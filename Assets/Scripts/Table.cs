using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class Table : MonoBehaviour
{
    public static Table Instance { get; private set; }

    [SerializeField] private GameObject tabletop;
    [SerializeField] private GameObject Deck;
    [SerializeField] private List<Chair> Chairs;

    private CardType currentType;
    private List<Card> cardsInPlay;
    private int numCards;

    private void Awake()
    {
        Instance = this;
        currentType = CardType.Any;
    }

    void Start()
    {
        DealCards();
    }

    #region Game Management
    private void DealCards()
    {
        int chairNum = 0;
        for (int i = 0; i < 52; i++)
        {
            GameObject Card = Deck.transform.GetChild(Random.Range(0, Deck.transform.childCount)).gameObject;
            Chairs[chairNum].DealtCard(Card);

            chairNum = (chairNum < 3) ? chairNum + 1 : 0;
        }
    }

    public void PlayCards(List<Card> cards, Chair chair)
    {
        if (!CheckIfCardsValid(cards))
            return;

        RemoveCardsOnTable();

        // Manage visual of card on table
        tabletop.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360f), 0));

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

    private void SetBoardState(CardType cardType, List<Card> cardsPlayed, int numberCards)
    {
        if (cardsPlayed != null)
        {
            Debug.Log("Card Type: " + cardType);
            foreach (Card card in cardsPlayed)
                Debug.Log(card.GetRank() + " " + card.GetSuit());
            Debug.Log("Num cards: " + numberCards);
            Debug.Log("================================");
        }

        currentType = cardType;
        cardsInPlay = cardsPlayed;
        numCards = numberCards;
    }

    public void RedrawHands()
    {
        RemoveCardsOnTable();

        for (int i = 0; i < 4; i++)
        {
            List<Transform> cards = new List<Transform>();
            foreach (Transform cardInHand in Chairs[i].GetHand().transform)
                cards.Add(cardInHand);

            foreach (Transform card in cards)
            {
                card.position = Vector3.zero;
                card.SetParent(Deck.transform);
                card.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }

        DealCards();
        SetBoardState(CardType.Any, null, 0);
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
        if (currentType == CardType.Any || (currentType == CardType.Single && cards[0].GetValue() > cardsInPlay[0].GetValue()))
        {
            SetBoardState(CardType.Single, cards, 1);
            return true;
        }

        return false;
    }

    private bool CheckValidDouble(List<Card> cards)
    {
        // If free move OR round is doubles + highest card played is higher than highest card on board
        if (currentType == CardType.Any || (currentType == CardType.Double && cards[1].GetValue() > cardsInPlay[1].GetValue()))
        {
            // Rank of each card is the same
            if (cards[0].GetRank() == cards[1].GetRank())
            {
                SetBoardState(CardType.Double, cards, 2);
                return true;
            }
        }

        return false;
    }

    private bool CheckValidTriple(List<Card> cards)
    {
        // If free move OR round is triples + highest card played is higher than highest card on board
        if (currentType == CardType.Any || (currentType == CardType.Triple && cards[2].GetValue() > cardsInPlay[2].GetValue()))
        {
            // Rank of each card is the same
            if (cards.All(card => card.GetRank() == cards[0].GetRank()))
            {
                SetBoardState(CardType.Triple, cards, 3);
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
            (currentType == CardType.Quadruple && cards[3].GetValue() > cardsInPlay[3].GetValue()) ||
            (cardsInPlay[0].GetRank() == Rank.Two && currentType == CardType.Single))
        {
            // Rank of each card is the same
            if (cards.All(card => card.GetRank() == cards[0].GetRank()))
            {
                SetBoardState(CardType.Quadruple, cards, 4);
                return true;
            }
        }

        return false;
    }

    private bool CheckValidStraight(List<Card> cards)
    {
        // If free move OR round is straights + highest card played is higher than highest card on board + num cards played is the same
        if (currentType == CardType.Any || 
            (currentType == CardType.Straight && cards[numCards - 1].GetValue() > cardsInPlay[numCards - 1].GetValue() && cards.Count == numCards))
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

            SetBoardState(CardType.Straight, cards, cards.Count);
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
            (currentType == CardType.Bomb && cards[numCards - 1].GetValue() > cardsInPlay[numCards - 1].GetValue() && cards.Count == numCards) ||
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

            SetBoardState(CardType.Bomb, cards, cards.Count);
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

}
