using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chair : MonoBehaviour, IInteractable
{
    [SerializeField] private Outline outline;
    [SerializeField] private GameObject sitPoint;
    [SerializeField] private GameObject exitPoint;
    [SerializeField] private GameObject hand;
    [SerializeField] private int chairID;

    [SerializeField] private List<Card> selectedCards;

    private bool hasPlayer;

    private float fanRadius = 0.15f;
    private float maxFanAngle = 67.5f;

    public void Highlight() { outline.enabled = true; }
    public void Unhighlight() { outline.enabled = false; }
    public void Interact(GameObject player)
    {
        player.GetComponent<Player>().SitOnChair(this);
        hasPlayer = true;
    }
    public GameObject GetSitPoint()
    {
        hasPlayer = true;
        return sitPoint;
    }
    public Vector3 GetExitPoint()
    {
        hasPlayer = false;
        return exitPoint.transform.position;
    }

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

    public void SelectedCard(Card card)
    {
        if (selectedCards.Contains(card))
            selectedCards.Remove(card);
        else
            selectedCards.Add(card);
    }

    public bool PlayCards()
    {
        if (selectedCards.Count > 0)
        {
            bool success = Table.Instance.PlayCards(selectedCards, this);
            ResetSelected();
            ArrangeCardsInFan();
            return success;
        }

        return false;
    }

    public bool PlayCards(List<Card> selectedCards)
    {
        if (selectedCards.Count > 0)
        {
            bool success = Table.Instance.PlayCards(selectedCards, this);
            ResetSelected();
            ArrangeCardsInFan();
            return success;
        }

        return false;
    }

    private void ResetSelected()
    {
        foreach (Card card in selectedCards)
            card.Selected = false;

        selectedCards.Clear();
    }

    public List<Card> GetHand()
    {
        List<Card> cards = new List<Card>();
        foreach (Transform cardInHand in hand.transform)
            if (cardInHand.GetComponent<Card>() != null)
                cards.Add(cardInHand.GetComponent<Card>());

        return cards;
    }

    public int GetChairID() => chairID;
    public bool HasPlayer() => hasPlayer;
}
