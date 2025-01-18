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
        Table.Instance.currentType.OnValueChanged += CurrentTypeChanged;
    }

    private void CurrentTypeChanged(CardType prev, CardType current)
    {
        cardType.text = current.ToString();
    }

    [ClientRpc]
    public void UpdateVisualClientRpc()
    {
        foreach (Transform child in cardImages)
        {
            if (child == imageTemplate) continue;

            Destroy(child.gameObject);
        }

        if (Table.Instance.GetCardsInPlay() != null)
        {
            foreach (CardData card in Table.Instance.GetCardsInPlay())
            {
                Transform imageTransform = Instantiate(imageTemplate, cardImages);
                imageTransform.gameObject.SetActive(true);
                imageTransform.GetComponent<Image>().sprite = Table.Instance.GetSpriteFromValue(card.value);
            }
        }
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
