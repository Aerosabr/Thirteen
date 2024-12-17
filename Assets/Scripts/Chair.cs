using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Chair : InteractableObject
{
    [SerializeField] private Outline outline;
    [SerializeField] private GameObject sitPoint;
    [SerializeField] private GameObject exitPoint;
    [SerializeField] private GameObject hand;
    [SerializeField] private int chairID;

    private Transform aiTransform;
    private PlayerType playerType;
    public bool inRound = true;
    public ulong playerID = ulong.MaxValue;

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
        Player player = playerObj.GetComponent<Player>();
        player.GetComponent<Player>().SitOnChair(NetworkObject);
        playerType = PlayerType.Player;
        PlayerOrderUI.Instance.ChairStateChangedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerExitServerRpc(NetworkObjectReference playerRef) => PlayerExitClientRpc(playerRef);

    [ClientRpc]
    private void PlayerExitClientRpc(NetworkObjectReference playerRef)
    {
        playerRef.TryGet(out NetworkObject playerObj);
        playerID = ulong.MaxValue;
        Player player = playerObj.GetComponent<Player>();
        player.GetComponent<Player>().ExitChair();
        playerType = PlayerType.None;
        PlayerOrderUI.Instance.ChairStateChangedServerRpc();
    }

    [ServerRpc]
    public void AlternateInteractServerRpc() => AlternateInteractClientRpc();

    [ClientRpc]
    private void AlternateInteractClientRpc()
    {
        if (playerType == PlayerType.AI)
        {
            Destroy(aiTransform);
            playerType = PlayerType.None;
            PlayerOrderUI.Instance.ChairStateChangedServerRpc();
        }
        else
        {
            aiTransform = Instantiate(PlayerManager.Instance.GetAIPrefab());
            aiTransform.GetComponent<AI>().SitOnChair(NetworkObject);
            PlayerInfo aiInfo = new PlayerInfo();
            aiInfo.playerName = "AI";
            aiInfo.modelNum = aiTransform.GetComponent<AI>().modelNum;
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

    public int GetChairID() => chairID;
    public PlayerType GetPlayerType() => playerType;
}
