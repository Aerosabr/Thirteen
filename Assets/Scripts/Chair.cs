using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;

public class Chair : InteractableObject
{
    [SerializeField] private Outline outline;
    [SerializeField] private GameObject sitPoint;
    [SerializeField] private GameObject exitPoint;
    [SerializeField] private GameObject hand;
    [SerializeField] private int chairID;

    private PlayerType playerType;
    public bool inRound = true;
    public ulong playerID = ulong.MaxValue;
    private bool canPlay = false;
    private bool isReady = false;
    private Player player;
    [SerializeField] private List<Card> selectedCards;

    private float fanRadius = 0.15f;
    private float maxFanAngle = 67.5f;

    private void Start()
    {
        interactType = InteractType.Chair;
        playerType = PlayerType.None;
    }

    public override bool Highlight(GameObject player) 
    {
        if (playerType != PlayerType.Player)
        {
            outline.enabled = true;
            return true;
        }

        return false;
    }
    public override void Unhighlight() { outline.enabled = false; }

    public override void Interact(NetworkObjectReference playerRef) => InteractServerRpc(playerRef);

    [ServerRpc(RequireOwnership = false)]
    public void InteractServerRpc(NetworkObjectReference playerRef)
    {
        if (playerType != PlayerType.Player)
        {
            InteractClientRpc(playerRef);
        }
    }

    [ClientRpc]
    private void InteractClientRpc(NetworkObjectReference playerRef)
    {
        playerRef.TryGet(out NetworkObject playerObj);
        playerID = playerObj.OwnerClientId;
        player = playerObj.GetComponent<Player>();
        player.GetComponent<Player>().SitOnChair(NetworkObject);
        playerType = PlayerType.Player;
        player.GetComponent<Human>().OnSpaceBarPressed += Human_OnSpaceBarPressed;
        PlayerOrderUI.Instance.ChairStateChangedServerRpc();
    }

    private void Human_OnSpaceBarPressed(object sender, System.EventArgs e)
    {
        player.GetComponent<Human>().ThrowingCardServerRpc();

        if (canPlay)
        {
            if (selectedCards.Count == 0)
            {
                if (Table.Instance.GetCurrentType() != CardType.LowestThree && Table.Instance.GetCurrentType() != CardType.Any)
                {
                    Table.Instance.SkipTurn();
                    canPlay = false;
                }
            }
            else if (Table.Instance.CheckIfCardsValid(selectedCards))
            {
                canPlay = false;
                player.GetComponent<Human>().ThrowingCardServerRpc();
            }
        }

        /*
        if (StartNextGameUI.Instance.GetAwaitingReady())
            StartNextGameUI.Instance.ReadyUp(this);
        */
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerExitServerRpc() => PlayerExitClientRpc();

    [ClientRpc]
    private void PlayerExitClientRpc()
    {
        isReady = false;
        playerID = ulong.MaxValue;
        player.GetComponent<Player>().ExitChair();
        playerType = PlayerType.None;
        player.GetComponent<Human>().OnSpaceBarPressed -= Human_OnSpaceBarPressed;
        player = null;
        PlayerOrderUI.Instance.ChairStateChangedServerRpc();
    }

    [ServerRpc]
    public void AlternateInteractServerRpc() => AlternateInteractClientRpc();

    [ClientRpc]
    private void AlternateInteractClientRpc()
    {
        if (playerType == PlayerType.AI)
        {
            Table.Instance.RemoveAI(chairID);
            playerType = PlayerType.None;
            PlayerOrderUI.Instance.ChairStateChangedServerRpc();
        }
        else
        {
            Table.Instance.SpawnAI(chairID);
            playerType = PlayerType.AI;
            PlayerOrderUI.Instance.ChairStateChangedServerRpc();
        }
    }

    public GameObject GetSitPoint() => sitPoint;
    public Vector3 GetExitPoint() => exitPoint.transform.position;

    public void DealtCard(GameObject card)
    {
        if (card.GetComponent<Card>() != null)
        {
            card.transform.SetParent(hand.transform);
            SortCardsByValue();
            ArrangeCardsInFan();
        }
        else
            Debug.Log("Object is not a card");
    }

    private void ArrangeCardsInFan()
    {
        int childCount = hand.transform.childCount;

        if (childCount == 0)
            return;

        float angleStep = (childCount > 1) ? maxFanAngle / (childCount - 1) : 0;
        float startAngle = -maxFanAngle / 2;

        for (int i = 0; i < childCount; i++)
        {
            float angle = (childCount > 1) ? startAngle + i * angleStep : 0;
            float rad = Mathf.Deg2Rad * angle;

            Vector3 cardPosition = new Vector3(Mathf.Sin(rad) * fanRadius, i * 0.0001f, Mathf.Cos(rad) * fanRadius);

            Transform card = hand.transform.GetChild(i);
            card.gameObject.GetComponent<Card>().handPos = new Vector3(Mathf.Sin(rad), i * 0.0001f, Mathf.Cos(rad));
            card.localPosition = cardPosition;
            card.localRotation = Quaternion.Euler(0, angle, 0);
        }
    }

    private void SortCardsByValue()
    {
        List<Card> cards = GetHand();

        cards = cards.OrderBy(child => child.GetValue()).ToList();

        for (int i = 0; i < cards.Count; i++)
            cards[i].transform.SetSiblingIndex(i);
    }

    public void CardsPlayed() => ArrangeCardsInFan(); // Refan cards in hand

    public List<Card> GetHand()
    {
        List<Card> cards = new List<Card>();
        foreach (Transform cardInHand in hand.transform)
            if (cardInHand.GetComponent<Card>() != null)
                cards.Add(cardInHand.GetComponent<Card>());

        return cards;
    }

    public bool GetReadyState() => isReady;
    public int GetChairID() => chairID;
    public PlayerType GetPlayerType() => playerType;
}
