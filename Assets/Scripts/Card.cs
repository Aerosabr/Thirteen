using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Card : InteractableObject
{

    [SerializeField] private Rank Rank;
    [SerializeField] private Suit Suit;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private GameObject highlight;
    [SerializeField] private Sprite cardSprite;

    public GameObject emptyCard;
    public Vector3 handPos;
    public bool Selected;

    private void Start()
    {
        interactType = InteractType.Card;
    }

    public override bool Highlight(GameObject player)
    {
        highlight.SetActive(true);
        return true;
    }

    public override void Unhighlight()
    {
        highlight.SetActive(false);
    }

    public override void Interact(NetworkObjectReference playerRef) => InteractServerRpc(playerRef);

    [ServerRpc(RequireOwnership = false)]
    public void InteractServerRpc(NetworkObjectReference playerRef)
    {
        playerRef.TryGet(out NetworkObject playerObj);
        Player player = playerObj.GetComponent<Player>();
        if (!Selected)
        {
            transform.localPosition = new Vector3(handPos.x * 0.175f, handPos.y, handPos.z * 0.175f);
            player.GetComponent<Human>().SelectedCard(this);
            Selected = true;
        }
        else
        {
            transform.localPosition = new Vector3(handPos.x * 0.15f, handPos.y, handPos.z * 0.15f);
            player.GetComponent<Human>().SelectedCard(this);
            Selected = false;
        }
    }

    [ClientRpc]
    public void SetPositionClientRpc(Vector3 handPos, Vector3 pos, Quaternion rotation)
    {
        this.handPos = handPos;
        transform.localPosition = pos;
        transform.localRotation = rotation;
    }

    public int GetValue() => (int)Rank + (int)Suit;
    public Sprite GetSprite() => cardSprite;
    public Rank GetRank() => Rank;
    public Suit GetSuit() => Suit;

}

[System.Serializable]
public struct CardData
{
    public Rank rank;
    public Suit suit;
    public int value;

    public CardData(Rank rank, Suit suit)
    {
        this.rank = rank;
        this.suit = suit;
        value = (int)rank + (int)suit;
    }
}

[System.Serializable]
public struct CardCombo
{
    public List<CardData> cards;

    public CardCombo(List<CardData> cards)
    { 
        this.cards = cards; 
    }
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