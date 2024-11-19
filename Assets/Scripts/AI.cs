using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Playables;

public class AI : Player
{
    [SerializeField] private List<CardData> hand = new List<CardData>();
    [SerializeField] private List<CardData> isolatedSingles = new List<CardData>();
    [SerializeField] private List<CardData> twos = new List<CardData>();
    [SerializeField] private List<CardData> cardsToBePlayed = new List<CardData>();

    public override void SitOnChair(Chair chair)
    {
        transform.position = chair.GetSitPoint().transform.position;
        transform.rotation = chair.GetSitPoint().transform.rotation;
        this.chair = chair;

        playerVisual.PlayAnimation("Sitting");
    }

    public override void InitializePlayer(int playerPos)
    {
        playerID = playerPos;

        Table.Instance.OnPlayerTurn += Table_OnPlayerTurn;
        Table.Instance.OnCardsDealt += Table_OnCardsDealt;

        Table.Instance.GetChair(playerPos).Interact(gameObject);
    }

    private void Table_OnCardsDealt(object sender, System.EventArgs e) => ProcessHand();

    private void Table_OnPlayerTurn(object sender, Table.OnPlayerTurnEventArgs e)
    {
        if (e.currentPlayer == playerID)
            StartCoroutine(TakeAction());
    }

    private IEnumerator TakeAction()
    {
        yield return new WaitForSeconds(Random.Range(2f, 5f));

        switch (Table.Instance.GetCurrentType())
        {
            case CardType.LowestThree:

                break;
            case CardType.Any:

                break;

            case CardType.Single:
                PlaySingle();
                break;

            case CardType.Double:

                break;
            case CardType.Triple:

                break;
            case CardType.Quadruple:

                break;
            case CardType.Straight:

                break;
            case CardType.Bomb:

                break;

        }
    }

    private void PlayCards()
    {
        Debug.Log(playerID + ": Playing");
        Debug.Log("================================");
        
        playerVisual.PlayAnimation("Throwing");
    }

    public override void CardThrown()
    {
        List<Card> selectedCards = new List<Card>();

        foreach (CardData cardData in cardsToBePlayed)
        {
            List<Card> cardsInHand = chair.GetHand();
            foreach (Card card in cardsInHand)
            {
                if (card.GetValue() == cardData.value)
                    selectedCards.Add(card);
            }
        }

        cardsToBePlayed.Clear();
        Table.Instance.CheckIfCardsValid(selectedCards);
        Table.Instance.PlayCards(selectedCards);
        chair.CardsPlayed();
        ProcessHand();
    }

    #region Hand Processing
    private void ProcessHand()
    {
        List<Card> cardsInHand = chair.GetHand();
        if (cardsInHand.Count == 0) // Player's hand is empty, alert table 
        {
            Table.Instance.SkipTurn();
            return;
        }

        // Load hand into CardData list
        hand.Clear();
        twos.Clear();

        foreach (Card card in chair.GetHand())
            hand.Add(new CardData(card.GetRank(), card.GetSuit()));

        // Split twos from hand
        for (int i = hand.Count - 1; i >= 0; i--)
        {
            if (hand[i].rank == Rank.Two)
            {
                twos.Add(hand[i]);
                hand.Remove(hand[i]);
            }
        }

        // Get cards that are not apart of any combos 
        isolatedSingles = FindIsolatedSingles(hand);
    }

    // NOTE: ensure list being passed in contains no Twos
    private List<CardData> FindIsolatedSingles(List<CardData> cardDatas)
    {
        // Dictionary to count quantity of each rank
        var rankQuantities = cardDatas.GroupBy(card => (int)card.rank).ToDictionary(group => group.Key, group => group.Count());

        // Identify isolated singles
        var isolatedSingles = cardDatas.Where(card =>
        {
            return rankQuantities[(int)card.rank] == 1 && // Only one card of this rank
            !(rankQuantities.ContainsKey((int)card.rank - 4) && rankQuantities.ContainsKey((int)card.rank + 4)) && // No adjacent cards
            !(rankQuantities.ContainsKey((int)card.rank - 4) && rankQuantities.ContainsKey((int)card.rank - 8)) && // No previous two cards
            !(rankQuantities.ContainsKey((int)card.rank + 4) && rankQuantities.ContainsKey((int)card.rank + 8));  // No next two cards
        }).ToList();

        return isolatedSingles;
    }

    private void PlaySingle()
    {
        int cardInPlayValue = Table.Instance.GetCardsInPlay()[0].GetValue();
        foreach (CardData card in hand)
        {
            if (card.value > cardInPlayValue)
            {
                List<CardData> cardToPlay = new List<CardData>() { card };
                if (FindIsolatedSingles(hand.Except(cardToPlay).ToList()).Count <= isolatedSingles.Count)
                {
                    cardsToBePlayed = cardToPlay;
                    PlayCards();
                    return;
                }             
            }
        }

        Table.Instance.SkipTurn();
    }

    private List<CardCombo> FindDouble(List<CardData> cardDatas)
    {
        // Filter hand for doubles
        var pairs = cardDatas.GroupBy(card => card.rank).Where(group => group.Count() == 2)
            .Select(group => new CardCombo(group.ToList())).ToList();

        return pairs;
    }

    private List<CardCombo> FindTriple(List<CardData> cardDatas)
    {
        // Filter hand for triples
        var triples = cardDatas.GroupBy(card => card.rank).Where(group => group.Count() == 3)
            .Select(group => new CardCombo(group.ToList())).ToList();

        return triples;
    }

    private List<CardCombo> FindQuadruples(List<CardData> cardDatas)
    {
        // Filter hand for quadruples
        var quads = cardDatas.GroupBy(card => card.rank).Where(group => group.Count() == 4)
            .Select(group => new CardCombo(group.ToList())) .ToList();

        return quads;
    }

    private List<CardCombo> FindStraights(List<CardData> cardDatas, int length)
    {
        // Sort cards by rank, ignoring suit
        var sortedCards = cardDatas.OrderBy(card => (int)card.rank).ToList();

        var straights = new List<CardCombo>();
        var currentStraight = new List<CardData>();

        for (int i = 0; i < sortedCards.Count; i++)
        {
            // Add the current card to the straight
            if (currentStraight.Count == 0 || (int)sortedCards[i].rank == (int)currentStraight.Last().rank + 4)
            {
                currentStraight.Add(sortedCards[i]);

                // If finding all straights, save each straight of at least 3 cards
                if (length == -1 && currentStraight.Count >= 3)
                {
                    straights.Add(new CardCombo(new List<CardData>(currentStraight)));
                }

                // Save the straight if it matches the required length
                if (length != -1 && currentStraight.Count == length)
                {
                    straights.Add(new CardCombo(new List<CardData>(currentStraight)));
                    currentStraight.RemoveAt(0); // Slide the window for overlapping straights
                }
            }
            else if ((int)sortedCards[i].rank != (int)currentStraight.Last().rank)
            {
                // Reset the straight if the sequence is broken
                currentStraight.Clear();
                currentStraight.Add(sortedCards[i]);
            }
        }

        return straights;
    }



    private List<CardCombo> FindBombs(List<CardData> cardDatas, int length)
    {
        // Group cards by rank and filter for pairs
        var pairs = cardDatas.GroupBy(card => card.rank).Where(group => group.Count() == 2)
            .OrderBy(group => (int)group.Key).Select(group => group.ToList()).ToList();

        var consecutivePairs = new List<CardCombo>();
        var currentCombo = new List<CardData>();

        for (int i = 0; i < pairs.Count; i++)
        {
            // Add the current pair to the current sequence
            if (currentCombo.Count == 0 ||
                (int)pairs[i][0].rank == (int)pairs[i - 1][0].rank + 1)
            {
                currentCombo.AddRange(pairs[i]);

                // If we reached the desired sequence length, save it
                if (currentCombo.Count == length * 2) // Each pair has 2 cards
                {
                    consecutivePairs.Add(new CardCombo(new List<CardData>(currentCombo)));
                    currentCombo.Clear(); // Reset for a new sequence
                }
            }
            else
            {
                // Reset the sequence if the consecutive order breaks
                currentCombo.Clear();
                currentCombo.AddRange(pairs[i]);
            }
        }

        return consecutivePairs;
    }
    #endregion
}
