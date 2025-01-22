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

    private NetworkVariable<PlayerType> playerType = new NetworkVariable<PlayerType>();
    public bool inRound = true;
    public ulong playerID = ulong.MaxValue;
    private NetworkVariable<bool> canPlay = new NetworkVariable<bool>();
    private Player player;
    [SerializeField] private List<Card> selectedCards;

    private float fanRadius = 0.15f;
    private float maxFanAngle = 67.5f;

    private void Start()
    {
        interactType = InteractType.Chair;
        if (IsServer)
        {
            Table.Instance.OnPlayerTurn += Table_OnPlayerTurn;
            playerType.Value = PlayerType.None;
        }
    }

    private void Table_OnPlayerTurn(object sender, Table.OnPlayerTurnEventArgs e)
    {
        if (e.currentPlayer == chairID && PlayerManager.Instance.CheckIfServer())
            canPlay.Value = true;
    }

    #region Object Highlighting
    public override bool Highlight(GameObject player) 
    {
        if (playerType.Value != PlayerType.Player)
        {
            outline.enabled = true;
            return true;
        }

        return false;
    }
    public override void Unhighlight() { outline.enabled = false; }
    #endregion

    #region Sit on Chair
    public override void Interact(NetworkObjectReference playerRef)
    {
        if (playerType.Value == PlayerType.None)
            InteractServerRpc(playerRef);
    }

    [ServerRpc(RequireOwnership = false)]
    public void InteractServerRpc(NetworkObjectReference playerRef)
    {
        InteractClientRpc(playerRef);
        playerType.Value = PlayerType.Player;
        Table.Instance.ChairStateChangedServerRpc();
        player.GetComponent<Human>().ToggleGameUIClientRpc(true);
    }

    [ClientRpc]
    private void InteractClientRpc(NetworkObjectReference playerRef)
    {
        playerRef.TryGet(out NetworkObject playerObj);
        playerID = playerObj.OwnerClientId;
        player = playerObj.GetComponent<Player>();
        player.GetComponent<Player>().SitOnChair(NetworkObject);
        player.GetComponent<Human>().OnSpaceBarPressed += Human_OnSpaceBarPressed;
    }
    #endregion

    #region Exit chair
    [ServerRpc(RequireOwnership = false)]
    public void PlayerExitServerRpc()
    {
        player.GetComponent<Human>().ToggleGameUIClientRpc(false);
        PlayerExitClientRpc();
        playerType.Value = PlayerType.None;
        Table.Instance.ChairStateChangedServerRpc();
    }

    [ClientRpc]
    private void PlayerExitClientRpc()
    {
        playerID = ulong.MaxValue;
        player.GetComponent<Player>().ExitChair();
        player.GetComponent<Human>().OnSpaceBarPressed -= Human_OnSpaceBarPressed;
        player = null;
    }
    #endregion

    #region Spawn AI
    [ServerRpc]
    public void SpawnAIServerRpc()
    {
        if (playerType.Value == PlayerType.AI)
        {
            Table.Instance.RemoveAIServerRpc(chairID);
            playerType.Value = PlayerType.None;
        }
        else
        {
            Table.Instance.SpawnAIServerRpc(chairID);
            playerType.Value = PlayerType.AI;
        }

        Table.Instance.ChairStateChangedServerRpc();
    }
    #endregion

    #region Hand Interaction
    public void SelectedCard(Card card)
    {
        if (selectedCards.Contains(card))
            selectedCards.Remove(card);
        else
            selectedCards.Add(card);
    }

    private void Human_OnSpaceBarPressed(object sender, System.EventArgs e)
    {
        SpaceBarPressedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpaceBarPressedServerRpc()
    {
        if (canPlay.Value)
        {
            if (selectedCards.Count == 0)
            {
                if (Table.Instance.GetCurrentType() != CardType.LowestThree && Table.Instance.GetCurrentType() != CardType.Any)
                {
                    Table.Instance.SkipTurn();
                    canPlay.Value = false;
                }
            }
            else if (Table.Instance.CheckIfCardsValid(selectedCards))
            {
                canPlay.Value = false;
                player.GetComponent<Human>().ThrowingCardServerRpc();
            }
        }

        if (Table.Instance.GetAwaitingReady())
            Table.Instance.ReadyUpServerRpc(chairID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayHandServerRpc()
    {
        Table.Instance.PlayCardsServerRpc(chairID);
        selectedCards.Clear();
    }
    #endregion

    public GameObject GetSitPoint() => sitPoint;
    public Vector3 GetExitPoint() => exitPoint.transform.position;

    public void DealtCard(GameObject card)
    {
        if (card.GetComponent<Card>() != null)
        {
            card.transform.SetParent(hand.transform);

            SortCardsByValue();
            //ArrangeCardsInFan();
        }
        else
            Debug.Log("Object is not a card");
    }

    [ClientRpc]
    public void DealtCardClientRpc(NetworkObjectReference cardRef)
    {
        cardRef.TryGet(out NetworkObject card);
        if (card.GetComponent<Card>() != null)
        {
            if (IsServer)
                card.transform.SetParent(hand.transform);

            card.gameObject.layer = LayerMask.NameToLayer("Player" + chairID);
            SortCardsByValue();
        }
        else
            Debug.Log("Object is not a card");
    }

    [ServerRpc(RequireOwnership = false)]
    public void ArrangeCardsInFanServerRpc()
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

            Vector3 cardPosition = new Vector3(Mathf.Sin(rad) * fanRadius, i * 0.002f, Mathf.Cos(rad) * fanRadius);

            Card card = hand.transform.GetChild(i).GetComponent<Card>();
            card.SetPositionClientRpc(new Vector3(Mathf.Sin(rad), i * 0.002f, Mathf.Cos(rad)), cardPosition, Quaternion.Euler(0, angle, 0));
        }
    }

    private void SortCardsByValue()
    {
        List<Card> cards = GetHand();

        cards = cards.OrderBy(child => child.GetValue()).ToList();

        for (int i = 0; i < cards.Count; i++)
            cards[i].transform.SetSiblingIndex(i);
    }

    public void CardsPlayed() => ArrangeCardsInFanServerRpc(); // Refan cards in hand

    public List<Card> GetHand()
    {
        List<Card> cards = new List<Card>();
        foreach (Transform cardInHand in hand.transform)
            if (cardInHand.GetComponent<Card>() != null)
                cards.Add(cardInHand.GetComponent<Card>());

        return cards;
    }

    public int GetChairID() => chairID;
    public PlayerType GetPlayerType() => playerType.Value;
    public List<Card> GetSelectedCards() => selectedCards;
    public void SetSelectedCards(List<Card> cards) => selectedCards = cards;
    public void ClearSelectedCards() => selectedCards.Clear();
}
