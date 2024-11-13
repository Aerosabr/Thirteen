using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Table : MonoBehaviour
{
    public static Table Instance { get; private set; }

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
}
