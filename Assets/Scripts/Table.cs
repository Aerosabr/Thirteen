using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class Table : NetworkBehaviour
{
    public static Table Instance { get; private set; }

    public event EventHandler<OnPlayerTurnEventArgs> OnPlayerTurn;
    public class OnPlayerTurnEventArgs : EventArgs
    {
        public int currentPlayer;
    }

    [SerializeField] private GameObject tabletop;
    [SerializeField] private GameObject Deck;
    [SerializeField] private List<Chair> Chairs;

    [SerializeField] private List<Sprite> cardSprites;

    private List<int> playersReady = new List<int>();
    private List<Scores> scores = new List<Scores>();
    private Dictionary<int, Transform> spawnedAI = new Dictionary<int, Transform>();

    public NetworkVariable<CardType> currentType = new NetworkVariable<CardType>();
    public List<CardData> cardsInPlay = new List<CardData>();

    private int currentPlayer;
    private int lastPlayerPlayed;
    private int numPlayers;
    private int lowestCardValue;
    private int maxCards;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (IsServer)
            currentType.Value = CardType.None;
        //if (IsServer)
        //   StartGame();
    }

    #region Card Management
    [ServerRpc]
    private void DealCardsServerRpc()
    {
        int chairNum = 0;

        for (int i = 0; i < maxCards; i++)
        {
            Card Card = Deck.transform.GetChild(UnityEngine.Random.Range(0, Deck.transform.childCount)).GetComponent<Card>();
            Chairs[chairNum].DealtCardClientRpc(Card.NetworkObject);
            //Chairs[chairNum].DealtCard(Card.gameObject);
            //Card.GetComponent<Card>().emptyCard.layer = LayerMask.NameToLayer("Player" + (chairNum + 1) + "Blank");
            //Card.GetComponent<Card>().emptyCard.SetActive(true);
            //Card.layer = LayerMask.NameToLayer("Player" + (chairNum + 1));
            chairNum = (chairNum < numPlayers - 1) ? chairNum + 1 : 0;
        }

        foreach (Chair chair in Chairs)
        {
            if (chair.GetPlayerType() != PlayerType.None)
                chair.ArrangeCardsInFanServerRpc();
        }
        return;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayCardsServerRpc(int chairID)
    {
        Debug.Log("Play cards");
        RemoveCardsOnTable();
        lastPlayerPlayed = currentPlayer;

        // Manage visual of card on table
        tabletop.transform.rotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360f), 0));

        float cardWidthSpacing = 0.025f;
        float cardHeightSpacing = 0.0001f;

        List<Card> cards = Chairs[chairID - 1].GetSelectedCards();

        int numCards = cards.Count;
        float startPos = (numCards > 1) ? -(numCards / 2) * cardWidthSpacing : 0;
        
        for (int i = 0; i < numCards; i++)
        {
            cards[i].transform.SetParent(tabletop.transform);
            Vector3 cardPos = new Vector3(startPos + (cardWidthSpacing * i), i * cardHeightSpacing, 0);
            cards[i].transform.localPosition = cardPos;
            cards[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
            cards[i].gameObject.layer = LayerMask.NameToLayer("Card");
            cards[i].emptyCard.SetActive(false);
        }

        cardsInPlay = new List<CardData>(ConvertCardsToCardData(cards));
        foreach (CardData card in cardsInPlay)
        {
            Debug.Log("Card: " + card.value);
        }
        GameStateUI.Instance.UpdateGameStateUIServerRpc();

        if (Chairs[currentPlayer - 1].GetHand().Count == 0)
            EmptiedHandServerRpc();
        else
            DetermineCurrentPlayerServerRpc();
    }

    private void RemoveCardsOnTable()
    {
        if (tabletop.transform.childCount == 0)
            return;

        // Move all visible cards on table into deck
        List<Transform> cards = new List<Transform>();
        foreach (Transform playedCard in tabletop.transform)
            cards.Add(playedCard);

        foreach (Transform card in cards)
        {
            card.position = Vector3.zero;
            card.SetParent(Deck.transform); 
        }

        cardsInPlay.Clear();
    }

    private void TakeCardsFromHands()
    {
        for (int i = 0; i < 4; i++)
        {
            List<Card> cards = Chairs[i].GetHand();

            foreach (Card card in cards)
            {
                card.transform.position = Vector3.zero;
                card.transform.SetParent(Deck.transform);
                card.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }
    }

    private List<CardData> ConvertCardsToCardData(List<Card> cards)
    {
        List<CardData> cardDatas = new List<CardData>();

        foreach (Card card in cards)
            cardDatas.Add(new CardData(card.GetRank(), card.GetSuit()));

        return cardDatas;
    }

    private void SetBoardType(CardType cardType)
    {
        currentType.Value = cardType;
    }
    #endregion

    #region Game Management
    [ServerRpc(RequireOwnership = false)]
    public void ReadyUpServerRpc(int chairID)
    {
        if (GetNumPlayersAtTable() != 4)
            return;

        if (playersReady.Contains(chairID))
            playersReady.Remove(chairID);
        else
            playersReady.Add(chairID);

        StartNextGameUI.Instance.UpdateUIServerRpc();

        if (playersReady.Count == GetNumHumansAtTable())
        {
            StartGameServerRpc();
            playersReady.Clear();
        }
    }

    [ServerRpc]
    public void StartGameServerRpc()
    {
        ResetGameStateClientRpc();
        DealCardsServerRpc();

        if (scores.Count > 0) // If there was a winner last round, give them free move
            SetBoardType(CardType.Any);
        else // First round of the session, lowest three goes first
            SetBoardType(CardType.LowestThree);

        scores.Add(new Scores(numPlayers));
        GameStateUI.Instance.UpdateGameStateUIServerRpc();
        DetermineCurrentPlayerServerRpc();
        return;     
    }

    [ClientRpc]
    private void ResetGameStateClientRpc()
    {
        foreach (Chair chair in Chairs)
            chair.inRound = true;

        currentPlayer = 0;

        lastPlayerPlayed = 0;

        numPlayers = GetNumPlayersAtTable();

        lowestCardValue = 0;

        //maxCards = 52 - (52 % numPlayers);
        maxCards = 52;

        TakeCardsFromHands();
        RemoveCardsOnTable();
        PlayerOrderUI.Instance.ResetUI();
    }

    [ServerRpc]
    private void EndGameServerRpc()
    {
        SetBoardType(CardType.None);
        StartNextGameUI.Instance.UpdateUIServerRpc();
    }

    [ServerRpc]
    private void EndRoundServerRpc()
    {
        // Reset "skipped" players
        foreach (Chair chair in Chairs)
        {
            if (chair.GetHand().Count != 0)
            {
                chair.inRound = true;
                PlayerOrderUI.Instance.RemoveSkipOverlayClientRpc(chair.GetChairID());
            }
        }

        // Reset board state
        SetBoardType(CardType.Any);
        RemoveCardsOnTable();
        GameStateUI.Instance.UpdateGameStateUIServerRpc();
    }

    [ServerRpc]
    private void EmptiedHandServerRpc()
    {
        int gameNum = scores.Count - 1;
        Chairs[currentPlayer - 1].inRound = false;
        scores[gameNum].players[currentPlayer - 1] = scores[gameNum].players.Max() + 1;
        numPlayers--;

        PlayerOrderUI.Instance.PlayerHandEmptiedClientRpc(currentPlayer, scores[gameNum].players[currentPlayer - 1]);

        if (numPlayers == 1)
        {
            int remainingPlayer = scores[gameNum].players.FindIndex(player => player == 0);
            scores[gameNum].players[remainingPlayer] = scores[gameNum].players.Max() + 1;
            PlayerOrderUI.Instance.PlayerHandEmptiedClientRpc(remainingPlayer + 1, scores[gameNum].players[remainingPlayer]);
            EndGameServerRpc();
        }
        else
            DetermineCurrentPlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChairStateChangedServerRpc()
    {
        if (GetNumPlayersAtTable() != 4)
            playersReady.Clear();

        StartNextGameUI.Instance.UpdateUIServerRpc();
        PlayerOrderUI.Instance.ChairStateChangedServerRpc();
    }

    public int GetNumPlayersAtTable()
    {
        int numPlayers = 0;
        foreach (Chair chair in Chairs)
        {
            if (chair.GetPlayerType() == PlayerType.Player || chair.GetPlayerType() == PlayerType.AI)
                numPlayers++;
        }

        return numPlayers;
    }

    public int GetNumHumansAtTable()
    {
        int numHumans = 0;
        foreach (Chair chair in Chairs)
        {
            if (chair.GetPlayerType() == PlayerType.Player)
                numHumans++;
        }

        return numHumans;
    }

    public int GetNumHumansReady() => playersReady.Count;
    #endregion

    #region Turn Management
    public void SkipTurn()
    {
        Chairs[currentPlayer - 1].inRound = false;
        PlayerOrderUI.Instance.PlayerSkippedServerRpc(currentPlayer);
        DetermineCurrentPlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DetermineCurrentPlayerServerRpc()
    {
        Debug.Log("Current value: " + currentType.Value);
        switch (currentType.Value)
        {
            case CardType.Any: // Free move
                if (scores.Count > 0) // First move of game
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (scores[scores.Count - 2].players[i] == 1)
                            currentPlayer = i + 1;
                    }
                }
                else // First move of round
                    GetNextPlayerInRound();
                break;

            case CardType.LowestThree: // First move overall, search for hand with lowest three
                for (int j = 1; j < 53; j++)
                {
                    for (int i = 0; i < Chairs.Count; i++)
                    {
                        List<Card> cards = Chairs[i].GetHand();
                        foreach (Card card in cards)
                        {
                            if (card.GetValue() == j)
                            {
                                currentPlayer = i + 1;
                                OnPlayerTurn?.Invoke(this, new OnPlayerTurnEventArgs
                                {
                                    currentPlayer = currentPlayer
                                });
                                lowestCardValue = j;
                                return;
                            }
                        }
                    }
                }
                break;

            default: // Round in progress: get next player based on turn order
                GetNextPlayerInRound();
                break;
        }

        OnPlayerTurn?.Invoke(this, new OnPlayerTurnEventArgs
        {
            currentPlayer = currentPlayer
        });
    }

    private void GetNextPlayerInRound()
    {
        int maxPlayers = GetNumPlayersAtTable();
        for (int i = 0; i < maxPlayers; i++)
        {
            currentPlayer = GetNextInRotation();

            if (Chairs[currentPlayer - 1].inRound)
            {
                if (currentPlayer == lastPlayerPlayed)
                    EndRoundServerRpc();

                return;
            }

            if (currentPlayer == lastPlayerPlayed)
            {
                if (Chairs[currentPlayer - 1].GetHand().Count == 0)
                {
                    for (int j = 0; j < maxPlayers; j++)
                    {
                        currentPlayer = GetNextInRotation();

                        if (Chairs[currentPlayer - 1].GetHand().Count > 0)
                        {
                            EndRoundServerRpc();
                            return;
                        }
                    }
                }

                return;
            }
        }
    }
    
    private int GetNextInRotation()
    {
        int playerCount = GetNumPlayersAtTable();

        if (GameSettings.Instance.turnOrder == TurnOrder.Clockwise)
            return (currentPlayer % playerCount) + 1;
        //else if (GameSettings.Instance.turnOrder == TurnOrder.CounterClockwise)
        return (currentPlayer - 2 + playerCount) % playerCount + 1;
    }
    #endregion

    #region Card Processing
    public bool CheckIfCardsValid(List<Card> cards)
    {
        // First move of the session -> verify if cards played contains Three of Spades
        if (currentType.Value == CardType.LowestThree)
            if (!CheckLowestThree(cards))
                return false;

        cards = cards.OrderBy(card => card.GetValue()).ToList();

        // Process cards to be played based on number selected
        switch (cards.Count)
        {
            case 1: // Single
                if (CheckValidSingle(cards)) return true;
                break;

            case 2: // Double
                if (CheckValidDouble(cards)) return true;
                break;

            case 3: // Triple, Straight
                if (CheckValidTriple(cards)) return true;
                if (CheckValidStraight(cards)) return true;
                break;

            case 4: // Quadruple, Straight
                if (CheckValidQuadruple(cards)) return true;
                if (CheckValidStraight(cards)) return true;
                break;

            default: // Straight, Bomb
                if (CheckValidBomb(cards)) return true;
                if (CheckValidStraight(cards)) return true;
                break;
        }

        return false;
    }

    private bool CheckValidSingle(List<Card> cards)
    {
        // If free move OR round is singles + card played is higher than card on board
        if (currentType.Value == CardType.Any || 
            currentType.Value == CardType.LowestThree || 
            (currentType.Value == CardType.Single && cards[0].GetValue() > cardsInPlay[0].value))
        {
            SetBoardType(CardType.Single);
            return true;
        }

        return false;
    }

    private bool CheckValidDouble(List<Card> cards)
    {
        // If free move OR round is doubles + highest card played is higher than highest card on board
        if (currentType.Value == CardType.Any || 
            currentType.Value == CardType.LowestThree || 
            (currentType.Value == CardType.Double && cards[1].GetValue() > cardsInPlay[1].value))
        {
            // Rank of each card is the same
            if (cards[0].GetRank() == cards[1].GetRank())
            {
                SetBoardType(CardType.Double);
                return true;
            }
        }

        return false;
    }

    private bool CheckValidTriple(List<Card> cards)
    {
        // If free move OR round is triples + highest card played is higher than highest card on board
        if (currentType.Value == CardType.Any || 
            currentType.Value == CardType.LowestThree || 
            (currentType.Value == CardType.Triple && cards[2].GetValue() > cardsInPlay[2].value))
        {
            // Rank of each card is the same
            if (cards.All(card => card.GetRank() == cards[0].GetRank()))
            {
                SetBoardType(CardType.Triple);
                return true;
            }
        }

        return false;
    }

    private bool CheckValidQuadruple(List<Card> cards)
    {
        // If free move OR round is quadruples + highest card played is higher than highest card on board
        // OR card on board consists of a single Two
        if (currentType.Value == CardType.Any || 
            currentType.Value == CardType.LowestThree || 
            (currentType.Value == CardType.Quadruple && cards[3].GetValue() > cardsInPlay[3].value) ||
            (cardsInPlay[0].rank == Rank.Two && currentType.Value == CardType.Single))
        {
            // Rank of each card is the same
            if (cards.All(card => card.GetRank() == cards[0].GetRank()))
            {
                SetBoardType(CardType.Quadruple);
                return true;
            }
        }

        return false;
    }

    private bool CheckValidStraight(List<Card> cards)
    {
        // If free move OR round is straights + highest card played is higher than highest card on board + num cards played is the same
        if (currentType.Value == CardType.Any || 
            currentType.Value == CardType.LowestThree || 
            (currentType.Value == CardType.Straight && cards[cardsInPlay.Count - 1].GetValue() > cardsInPlay[cardsInPlay.Count - 1].value && cards.Count == cardsInPlay.Count))
        {
            // Disapprove if cards played contains a Two
            foreach (Card card in cards)
                if (card.GetRank() == Rank.Two)
                    return false;

            // Check if each card is a sequence
            int rank = (int)cards[0].GetRank();
            for (int i = 1; i < cards.Count; i++)
            {
                // Disapprove if next card is not the next rank from current
                if ((int)cards[i].GetRank() != rank + 4)
                    return false;

                rank += 4;
            }

            SetBoardType(CardType.Straight);
            return true;
        }
        return false;
    }

    private bool CheckValidBomb(List<Card> cards)
    {
        // Number of cards played must be even
        if (cards.Count % 2 != 0)
            return false;

        // Check if all cards on board are twos
        bool cardsInPlayAreTwos = true;
        foreach (CardData cardData in cardsInPlay)
        {
            if (cardData.rank != Rank.Two)
            {
                cardsInPlayAreTwos = false;
                break;
            }
        }

        // If free move OR round is bombs + highest card played is higher than highest card on board + num cards played is the same
        // OR cards on board consists of Twos and num cards played meets required amount to bomb
        if (currentType.Value == CardType.Any || 
            currentType.Value == CardType.LowestThree || 
            (currentType.Value == CardType.Bomb && cards[cardsInPlay.Count - 1].GetValue() > cardsInPlay[cardsInPlay.Count - 1].value && cards.Count == cardsInPlay.Count) ||
            cardsInPlayAreTwos && cards.Count >= (cardsInPlay.Count * 2 + 4))
        {
            foreach (Card card in cards)
                if (card.GetRank() == Rank.Two)
                    return false;

            int rank = (int)cards[0].GetRank();
            for (int i = 0; i < cards.Count; i += 2)
            {      
                if ((int)cards[i].GetRank() != rank || (int)cards[i + 1].GetRank() != rank)
                    return false;

                rank += 4;
            }
            SetBoardType(CardType.Bomb);
            return true;
        }

        return false;
    }

    private bool CheckLowestThree(List<Card> cards)
    {
        // Approve if cards played contains Three of Spades
        foreach (Card card in cards)
            if (card.GetValue() == lowestCardValue) 
                return true;
        
        return false;
    }
    #endregion

    #region AI Spawning
    [ServerRpc]
    public void SpawnAIServerRpc(int chairID)
    {
        Transform aiTransform = Instantiate(PlayerManager.Instance.GetAIPrefab().prefab);
        aiTransform.GetComponent<NetworkObject>().Spawn(true);
        aiTransform.GetComponent<AI>().SitOnChair(Chairs[chairID - 1].NetworkObject);

        spawnedAI.Add(chairID, aiTransform);
    }

    [ServerRpc]
    public void RemoveAIServerRpc(int chairID)
    {
        spawnedAI[chairID].gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(spawnedAI[chairID].gameObject);
        spawnedAI.Remove(chairID);
    }
    #endregion

    public Sprite GetSpriteFromValue(int value) => cardSprites[value - 1];
    public bool GetAwaitingReady() => currentType.Value == CardType.None;
    public Chair GetChair(int chairNum) => Chairs[chairNum - 1];
    public CardType GetCurrentType() => currentType.Value;
    public List<CardData> GetCardsInPlay() => cardsInPlay;
    public int GetMaxCards() => maxCards;
    public Transform GetAIOnChair(int chairID) => spawnedAI[chairID];
    public int GetNumberCardsPlayed() => Deck.transform.childCount - (52 % maxCards);
}

[System.Serializable]
public struct Scores
{
    public List<int> players;

    public Scores(int numPlayers)
    {
        players = new List<int>();
        for (int i = 0; i < numPlayers; i++)
            players.Add(0);
    }
}
