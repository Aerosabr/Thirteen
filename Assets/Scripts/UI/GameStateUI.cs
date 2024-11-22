using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class GameStateUI : MonoBehaviour
{
    public static GameStateUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI cardType;
    [SerializeField] private Transform cardImages;
    [SerializeField] private Transform imageTemplate;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateVisual(List <Card> Cards = null, CardType cardType = CardType.Any)
    {
        foreach (Transform child in cardImages)
        {
            if (child == imageTemplate) continue;

            Destroy(child.gameObject);
        }

        if (Cards != null)
        {
            foreach (Card card in Cards)
            {
                Transform imageTransform = Instantiate(imageTemplate, cardImages);
                imageTransform.gameObject.SetActive(true);
                imageTransform.GetComponent<Image>().sprite = card.GetSprite();
            }
        }

        this.cardType.text = cardType.ToString();
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
