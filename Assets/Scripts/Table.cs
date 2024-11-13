using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Table : MonoBehaviour
{
    public static Table Instance { get; private set; }

    [SerializeField] private GameObject tabletop;
    [SerializeField] private GameObject Deck;
    [SerializeField] private List<Chair> Chairs; 

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        DealCards();
    }

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

    public void PlayCards(ref List<GameObject> cards)
    {
        RemoveCardsOnTable();

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
            cards[i].transform.rotation = Quaternion.Euler(Vector3.zero);
        }

        cards.Clear();
    }

    private void RemoveCardsOnTable()
    {
        if (tabletop.transform.childCount == 0)
            return;

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
}
