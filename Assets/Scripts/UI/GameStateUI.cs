using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;

public class GameStateUI : NetworkBehaviour
{
    public static GameStateUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI cardType;
    [SerializeField] private Transform cardImages;
    [SerializeField] private Transform imageTemplate;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Hide();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateGameStateUIServerRpc()
    {
        string currentType = Table.Instance.currentType.Value.ToString();
        Debug.Log("Current Type: " + currentType);

        List<int> cardValues = new List<int>();
        foreach (CardData card in Table.Instance.cardsInPlay)
            cardValues.Add(card.value);

        int[] cards = cardValues.ToArray();

        UpdateGameStateUIClientRpc(cards, currentType);
    }

    [ClientRpc]
    private void UpdateGameStateUIClientRpc(int[] cards, string currentType)
    {
        cardType.text = currentType;

        foreach (Transform child in cardImages)
        {
            if (child == imageTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (int card in cards)
        {
            Transform imageTransform = Instantiate(imageTemplate, cardImages);
            imageTransform.gameObject.SetActive(true);
            imageTransform.GetComponent<Image>().sprite = Table.Instance.GetSpriteFromValue(card);
        }
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
