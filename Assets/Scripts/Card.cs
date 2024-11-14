using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour, IInteractable
{

    [SerializeField] private Rank Rank;
    [SerializeField] private Suit Suit;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private GameObject highlight;

    public Vector3 handPos;

    public bool Selected;

    private void Start()
    {

    }

    public void Highlight()
    {
        highlight.SetActive(true);
    }

    public void Unhighlight()
    {
        highlight.SetActive(false);
    }

    public void Interact(GameObject chair)
    {
        if (!Selected)
        {
            transform.localPosition = new Vector3(handPos.x * 0.175f, handPos.y, handPos.z * 0.175f);
            chair.GetComponent<Chair>().SelectedCard(this);
            Selected = true;
        }
        else
        {
            transform.localPosition = new Vector3(handPos.x * 0.15f, handPos.y, handPos.z * 0.15f);
            chair.GetComponent<Chair>().SelectedCard(this);
            Selected = false;
        }
    }

    public int GetValue() => (int)Rank + (int)Suit;
    public Rank GetRank() => Rank;
    public Suit GetSuit() => Suit;
}

public enum Suit
{
    Spade = 1,
    Club = 2,
    Diamond = 3,
    Heart = 4,
}

public enum Rank
{
    Three = 0,
    Four = 4,
    Five = 8,
    Six = 12,
    Seven = 16,
    Eight = 20,
    Nine = 24,
    Ten = 28,
    Jack = 32,
    Queen = 36,
    King = 40,
    Ace = 44,
    Two = 48,
}

public enum CardType
{
    LowestThree,
    Any,
    Single,
    Double,
    Triple,
    Quadruple,
    Straight,
    Bomb,
    None,
}