using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Playables;
using static UnityEngine.Rendering.DebugUI;

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

    private void Table_OnCardsDealt(object sender, System.EventArgs e)
    {
        ProcessHand();
        
    }

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
                PlayLowestThree();
                break;

            case CardType.Any:
                PlayAny();
                break;

            case CardType.Single:
                PlaySingle();
                break;

            case CardType.Double:
                PlayDouble();
                break;

            case CardType.Triple:
                PlayTriple();
                break;

            case CardType.Quadruple:
                PlayQuadruple();
                break;

            case CardType.Straight:
                PlayStraight();
                break;

            case CardType.Bomb:
                PlayBomb();
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

    #region Turn Actions
    private void PlayLowestThree()
    {
        // Load all combos containing lowest three
        List<CardCombo> lowestThrees = new List<CardCombo>();
        lowestThrees.AddRange(FindBombs(hand.Except(twos).ToList(), -1).Where(combo => combo.cards.Any(card => card.value == 1)).ToList());
        lowestThrees.AddRange(FindStraights(hand.Except(twos).ToList(), -1).Where(combo => combo.cards.Any(card => card.value == 1)).ToList());
        lowestThrees.AddRange(FindDuplicates(hand, 2).Where(combo => combo.cards.Any(card => card.value == 1)).ToList());
        lowestThrees.AddRange(FindDuplicates(hand, 3).Where(combo => combo.cards.Any(card => card.value == 1)).ToList());
        lowestThrees.AddRange(FindDuplicates(hand, 4).Where(combo => combo.cards.Any(card => card.value == 1)).ToList());

        cardsToBePlayed = new List<CardData>() { hand[0] };
        float numIsolatedSingles = Mathf.Infinity;

        foreach (CardCombo combo in lowestThrees)
        {
            List<CardData> comboIsolatedSingles = FindIsolatedSingles(combo.cards);
            if (comboIsolatedSingles.Count <= numIsolatedSingles && combo.cards.Count > cardsToBePlayed.Count)
            {
                cardsToBePlayed = combo.cards;
                numIsolatedSingles = comboIsolatedSingles.Count;
            }
        }

        PlayCards();
    }

    private void PlayAny()
    {
        // Load all combos containing lowest card in hand
        int lowestCardValue = hand[0].value;
        List<CardCombo> lowestThrees = new List<CardCombo>();

        lowestThrees.AddRange(FindBombs(hand.Except(twos).ToList(), -1).Where(combo => combo.cards.Any(card => card.value == lowestCardValue)).ToList());
        lowestThrees.AddRange(FindStraights(hand.Except(twos).ToList(), -1).Where(combo => combo.cards.Any(card => card.value == lowestCardValue)).ToList());
        lowestThrees.AddRange(FindDuplicates(hand, 2).Where(combo => combo.cards.Any(card => card.value == lowestCardValue)).ToList());
        lowestThrees.AddRange(FindDuplicates(hand, 3).Where(combo => combo.cards.Any(card => card.value == lowestCardValue)).ToList());
        lowestThrees.AddRange(FindDuplicates(hand, 4).Where(combo => combo.cards.Any(card => card.value == lowestCardValue)).ToList());

        cardsToBePlayed = new List<CardData>() { hand[0] };
        float numIsolatedSingles = Mathf.Infinity;

        foreach (CardCombo combo in lowestThrees)
        {
            List<CardData> comboIsolatedSingles = FindIsolatedSingles(combo.cards);
            if (comboIsolatedSingles.Count <= numIsolatedSingles && combo.cards.Count > cardsToBePlayed.Count)
            {
                cardsToBePlayed = combo.cards;
                numIsolatedSingles = comboIsolatedSingles.Count;
            }
        }

        PlayCards();
    }

    private void PlaySingle()
    {
        List<Card> cardsInPlay = Table.Instance.GetCardsInPlay();
        int cardInPlayValue = cardsInPlay[0].GetValue();

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

        if (cardInPlayValue >= 49) // Card in play is a two
        {
            if (FindDuplicates(hand, 4).ToList().Count == 0)
                PlayBomb();
            else
                PlayQuadruple();

            return;
        }

        Table.Instance.SkipTurn();
    }

    private void PlayDouble()
    {
        int cardInPlayValue = Table.Instance.GetCardsInPlay()[1].GetValue();

        List<CardCombo> playableDoubles = FindDuplicates(hand, 2);

        if (playableDoubles.Count == 0)
        {
            Table.Instance.SkipTurn();
            return;
        }

        foreach (CardCombo combo in playableDoubles)
        {
            if (combo.cards[1].value > cardInPlayValue)
            {               
                if (FindIsolatedSingles(hand.Except(combo.cards).ToList()).Count <= isolatedSingles.Count)
                {
                    cardsToBePlayed = combo.cards;
                    PlayCards();
                    return;
                }
            }
        }
        
        Table.Instance.SkipTurn();
    }

    private void PlayTriple()
    {
        int cardInPlayValue = Table.Instance.GetCardsInPlay()[2].GetValue();

        List<CardCombo> playableTriples = FindDuplicates(hand, 3);

        if (playableTriples.Count == 0)
        {
            Table.Instance.SkipTurn();
            return;
        }

        foreach (CardCombo combo in playableTriples)
        {
            if (combo.cards[2].value > cardInPlayValue)
            {
                /*
                if (FindIsolatedSingles(hand.Except(combo.cards).ToList()).Count <= isolatedSingles.Count)
                {
                    cardsToBePlayed = combo.cards;
                    PlayCards();
                    return;
                }
                */
                cardsToBePlayed = combo.cards;
                PlayCards();
                return;
            }
        }

        Table.Instance.SkipTurn();
    }

    private void PlayQuadruple()
    {
        int cardInPlayValue = Table.Instance.GetCardsInPlay()[3].GetValue();

        List<CardCombo> playableQuadruples = FindDuplicates(hand, 4);

        if (playableQuadruples.Count == 0)
        {
            Table.Instance.SkipTurn();
            return;
        }

        foreach (CardCombo combo in playableQuadruples)
        {
            if (combo.cards[3].value > cardInPlayValue)
            {
                /*
                if (FindIsolatedSingles(hand.Except(combo.cards).ToList()).Count <= isolatedSingles.Count)
                {
                    cardsToBePlayed = combo.cards;
                    PlayCards();
                    return;
                }
                */
                cardsToBePlayed = combo.cards;
                PlayCards();
                return;
            }
        }

        Table.Instance.SkipTurn();
    }

    private void PlayStraight()
    {
        List<Card> cardsInPlay = Table.Instance.GetCardsInPlay();
        int straightLength = cardsInPlay.Count;
        int cardInPlayValue = cardsInPlay[straightLength - 1].GetValue();

        List<CardCombo> playableStraights = FindStraights(hand.Except(twos).ToList(), straightLength);

        if (playableStraights.Count == 0)
        {
            Table.Instance.SkipTurn();
            return;
        }

        foreach (CardCombo combo in playableStraights)
        {
            if (combo.cards[straightLength - 1].value > cardInPlayValue)
            {
                /*
                if (FindIsolatedSingles(hand.Except(combo.cards).ToList()).Count <= isolatedSingles.Count)
                {
                    cardsToBePlayed = combo.cards;
                    PlayCards();
                    return;
                }
                */
                cardsToBePlayed = combo.cards;
                PlayCards();
                return;
            }
        }

        Table.Instance.SkipTurn();
    }

    private void PlayBomb()
    {
        List<Card> cardsInPlay = Table.Instance.GetCardsInPlay();
        int cardInPlayValue = cardsInPlay[cardsInPlay.Count - 1].GetValue();

        List<CardCombo> playableBombs = new List<CardCombo>();
        if (Table.Instance.GetCurrentType() == CardType.Bomb)
        {
            playableBombs = FindBombs(hand, cardsInPlay.Count / 2);
        }
        else
        {
            playableBombs = FindBombs(hand, cardsInPlay.Count + 2);
        }

        if (playableBombs.Count == 0)
        {
            Table.Instance.SkipTurn();
            return;
        }

        foreach (CardCombo combo in playableBombs)
        {
            if (combo.cards[combo.cards.Count - 1].value > cardInPlayValue)
            {
                cardsToBePlayed = combo.cards;
                PlayCards();
                return;
            }
        }

        Table.Instance.SkipTurn();
    }

    private void PlayTwo()
    {
        // To be implemented later with more advanced AI
    }
    #endregion

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
                twos.Add(hand[i]);
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

    private List<CardCombo> FindDuplicates(List<CardData> cardDatas, int amount)
    {
        // Filter hand for cards with "amount" copies of the same rank
        var duplicates = cardDatas.GroupBy(card => card.rank).Where(group => group.Count() == amount)
            .Select(group => new CardCombo(group.ToList())).ToList();

        return duplicates;
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

    public static List<CardCombo> FindBombs(List<CardData> cardDatas, int length)
    {
        var pairs = new List<List<CardData>>();
        var groupedCards = cardDatas.GroupBy(card => card.rank).Where(group => group.Count() >= 2);

        // Create pairs manually from groups
        foreach (var group in groupedCards)
        {
            var cards = group.ToList();
            for (int i = 0; i < cards.Count - 1; i += 2)
            {
                pairs.Add(new List<CardData> { cards[i], cards[i + 1] });
            }
        }

        // Sort pairs by rank
        pairs = pairs.OrderBy(pair => pair[0].rank).ToList();

        var consecutivePairs = new List<CardCombo>();
        var currentCombo = new List<CardData>();

        for (int i = 0; i < pairs.Count; i++)
        {
            // Add the current pair to the current sequence
            if (currentCombo.Count == 0 || pairs[i][0].rank == pairs[i - 1][0].rank + 4)
            {
                currentCombo.AddRange(pairs[i]);

                // If length is -1, allow any bomb length of 3 or more pairs
                if (length == -1 && currentCombo.Count >= 6)
                {
                    consecutivePairs.Add(new CardCombo(new List<CardData>(currentCombo)));
                    currentCombo.Clear(); // Reset for new sequences
                }
                else if (length != -1 && currentCombo.Count == length * 2)
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

        // If length is -1, include any remaining combo of 3 or more pairs
        if (length == -1 && currentCombo.Count >= 6)
        {
            consecutivePairs.Add(new CardCombo(new List<CardData>(currentCombo)));
        }

        return consecutivePairs;
    }
    #endregion
}
