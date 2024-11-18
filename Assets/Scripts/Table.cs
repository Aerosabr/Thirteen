using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Table : MonoBehaviour
{
    public static Table Instance { get; private set; }

    public event EventHandler<OnPlayerTurnEventArgs> OnPlayerTurn;
    public class OnPlayerTurnEventArgs : EventArgs
    {
        public List<int> currentPlayer;
    }

    [SerializeField] private GameObject tabletop;
    [SerializeField] private GameObject Deck;
    [SerializeField] private List<Chair> Chairs;

    private List<Scores> scores = new List<Scores>();

    private CardType currentType;
    private List<Card> cardsInPlay;
    private List<int> currentPlayer = new List<int>();

    private void Awake()
    {
        Instance = this;
        currentType = CardType.Any;
    }

    void Start()
    {
        DealCards();
        StartRound();
    }

    #region Game Management
    private void DealCards()
    {
        int chairNum = 0;
        for (int i = 0; i < 52; i++)
        {
            GameObject Card = Deck.transform.GetChild(UnityEngine.Random.Range(0, Deck.transform.childCount)).gameObject;
            Chairs[chairNum].DealtCard(Card);

            chairNum = (chairNum < 3) ? chairNum + 1 : 0;
        }
    }

    public bool PlayCards(List<Card> cards, Chair chair)
    {
        if (!currentPlayer.Contains(chair.GetChairID()))
            return false;

        if (!CheckIfCardsValid(cards))
            return false;

        RemoveCardsOnTable();

        // Manage visual of card on table
        tabletop.transform.rotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360f), 0));

        float cardWidthSpacing = 0.025f;
        float cardHeightSpacing = 0.0001f;

        int numCards = cards.Count;
        float startPos = (numCards > 1) ? -(numCards / 2) * cardWidthSpacing : 0;
        
        for (int i = 0; i < numCards; i++)
        {
            cards[i].transform.SetParent(tabletop.transform);
            Vector3 cardPos = new Vector3(startPos + (cardWidthSpacing * i), i * cardHeightSpacing, 0);
            cards[i].transform.localPosition = cardPos;
            cards[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        DetermineNextPlayer();

        return true;
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
            Debug.Log(card.name);
            card.position = Vector3.zero;
            card.SetParent(Deck.transform); 
        }
    }

    private void SetBoardState(CardType cardType, List<Card> cardsPlayed)
    {
        if (cardsPlayed != null)
        {
            Debug.Log("Card Type: " + cardType);
            foreach (Card card in cardsPlayed)
                Debug.Log(card.GetRank() + " " + card.GetSuit());
            Debug.Log("================================");
        }

        currentType = cardType;
        cardsInPlay = cardsPlayed;
        GameStateUI.Instance.UpdateVisual(cardsPlayed, cardType);
    }

    public void RedrawHands()
    {
        // Move cards on table to deck
        RemoveCardsOnTable();

        // Move cards from hands to deck
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

        // Deal cards and reset game state
        DealCards();
        SetBoardState(CardType.Any, null);
        StartRound();
    }

    public void StartRound()
    {
        if (scores.Count > 0)
            SetBoardState(CardType.Any, null);
        else
            SetBoardState(CardType.LowestThree, null);

        DetermineNextPlayer();
    }

    private void DetermineNextPlayer()
    {
        switch (currentType)
        {
            case CardType.Any: // Free move: get winner of previous round
                currentPlayer.Clear();
                currentPlayer.Add(scores[scores.Count - 1].GetWinner());
                // visual stuff here
                break;
            case CardType.LowestThree: // First move of the game: find player with lowest three
                for (int i = 0; i < Chairs.Count; i++)
                {
                    List<Card> cards = Chairs[i].GetHand();
                    foreach (Card card in cards)
                    {
                        if (card.GetValue() == 1)
                        {
                            currentPlayer.Clear();
                            currentPlayer.Add(i + 1);
                            // visual
                        }
                        break;
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
        switch (GameSettings.Instance.turnOrder)
        {
            case TurnOrder.Clockwise:
                currentPlayer[0]++;
                break;

            case TurnOrder.CounterClockwise:
                currentPlayer[0]--;
                break;

            case TurnOrder.FirstPlay:

                break;
        }

        if (currentPlayer[0] > PlayerManager.Instance.Players.Count)
            currentPlayer[0] = 1;
        else if (currentPlayer[0] == 0)
            currentPlayer[0] = PlayerManager.Instance.Players.Count;
    }
    #endregion

    #region Card Processing
    public bool CheckIfCardsValid(List<Card> cards)
    {
        // First move of the session -> verify if cards played contains Three of Spades
        if (currentType == CardType.LowestThree)
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
        if (currentType == CardType.Any || currentType == CardType.LowestThree || (currentType == CardType.Single && cards[0].GetValue() > cardsInPlay[0].GetValue()))
        {
            SetBoardState(CardType.Single, cards);
            return true;
        }

        return false;
    }

    private bool CheckValidDouble(List<Card> cards)
    {
        // If free move OR round is doubles + highest card played is higher than highest card on board
        if (currentType == CardType.Any || currentType == CardType.LowestThree || (currentType == CardType.Double && cards[1].GetValue() > cardsInPlay[1].GetValue()))
        {
            // Rank of each card is the same
            if (cards[0].GetRank() == cards[1].GetRank())
            {
                SetBoardState(CardType.Double, cards);
                return true;
            }
        }

        return false;
    }

    private bool CheckValidTriple(List<Card> cards)
    {
        // If free move OR round is triples + highest card played is higher than highest card on board
        if (currentType == CardType.Any || currentType == CardType.LowestThree || (currentType == CardType.Triple && cards[2].GetValue() > cardsInPlay[2].GetValue()))
        {
            // Rank of each card is the same
            if (cards.All(card => card.GetRank() == cards[0].GetRank()))
            {
                SetBoardState(CardType.Triple, cards);
                return true;
            }
        }

        return false;
    }

    private bool CheckValidQuadruple(List<Card> cards)
    {
        // If free move OR round is quadruples + highest card played is higher than highest card on board
        // OR card on board consists of a single Two
        if (currentType == CardType.Any || 
            currentType == CardType.LowestThree || 
            (currentType == CardType.Quadruple && cards[3].GetValue() > cardsInPlay[3].GetValue()) ||
            (cardsInPlay[0].GetRank() == Rank.Two && currentType == CardType.Single))
        {
            // Rank of each card is the same
            if (cards.All(card => card.GetRank() == cards[0].GetRank()))
            {
                SetBoardState(CardType.Quadruple, cards);
                return true;
            }
        }

        return false;
    }

    private bool CheckValidStraight(List<Card> cards)
    {
        // If free move OR round is straights + highest card played is higher than highest card on board + num cards played is the same
        if (currentType == CardType.Any || 
            currentType == CardType.LowestThree || 
            (currentType == CardType.Straight && cards[cardsInPlay.Count - 1].GetValue() > cardsInPlay[cardsInPlay.Count - 1].GetValue() && cards.Count == cardsInPlay.Count))
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

            SetBoardState(CardType.Straight, cards);
            return true;
        }
        return false;
    }

    private bool CheckValidBomb(List<Card> cards)
    {
        // Number of cards played must be even
        if (cards.Count % 2 != 0)
            return false;

        // If free move OR round is bombs + highest card played is higher than highest card on board + num cards played is the same
        // OR cards on board consists of Twos and num cards played meets required amount to bomb
        if (currentType == CardType.Any || 
            currentType == CardType.LowestThree || 
            (currentType == CardType.Bomb && cards[cardsInPlay.Count - 1].GetValue() > cardsInPlay[cardsInPlay.Count - 1].GetValue() && cards.Count == cardsInPlay.Count) ||
            (cardsInPlay.All(card => card.GetRank() == Rank.Two) && cards.Count >= (cardsInPlay.Count * 2 + 4)))
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

            SetBoardState(CardType.Bomb, cards);
            return true;
        }

        return false;
    }

    private bool CheckLowestThree(List<Card> cards)
    {
        // Approve if cards played contains Three of Spades
        foreach (Card card in cards)
            if (card.GetValue() == 1) 
                return true;
        
        return false;
    }
    #endregion

    public Chair GetChair(int chairNum) => Chairs[chairNum - 1];
    public CardType GetCurrentType() => currentType;
    public List<Card> GetCardsInPlay() => cardsInPlay;
}

public struct Scores
{
    int Player1;
    int Player2;
    int Player3;
    int Player4;

    public int GetWinner()
    {
        if (Player1 == 1) return 1;
        if (Player2 == 1) return 2;
        if (Player3 == 1) return 3;
        return 4;
    }
}
