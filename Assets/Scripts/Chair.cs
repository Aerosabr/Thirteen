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

    private float fanRadius = 0.15f;
    private float maxFanAngle = 67.5f;

    public void Highlight() { outline.enabled = true; }
    public void Unhighlight() { outline.enabled = false; }
    public void Interact(GameObject player) { player.GetComponent<Player>().SitOnChair(sitPoint.transform.position); }
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
        List<GameObject> children = hand.GetComponentsInChildren<Transform>().Where(t => t != hand.transform).Select(t => t.gameObject).ToList();

        children = children.OrderBy(child => child.GetComponent<Card>().GetValue()).ToList();

        for (int i = 0; i < children.Count; i++)
            children[i].transform.SetSiblingIndex(i);
    }

    public void PlayCards()
    {

    }
}
