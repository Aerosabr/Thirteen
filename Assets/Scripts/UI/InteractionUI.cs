using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }

    [SerializeField] private GameObject eObject;
    [SerializeField] private TextMeshProUGUI eText;

    [SerializeField] private GameObject fObject;
    [SerializeField] private TextMeshProUGUI fText;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(InteractableObject interactObject, bool isServer)
    {
        Debug.Log("Showing");
        if (interactObject.GetInteractType() == InteractableObject.InteractType.Card) // Looking at card
        {
            if (interactObject.GetComponent<Card>().Selected)
            {
                eObject.SetActive(true);
                eText.text = "Deselect";
            }
            else
            {
                eObject.SetActive(true);
                eText.text = "Select";
            }
        }
        else // Looking at chair
        {
            if (isServer)
            {
                if (interactObject.GetComponent<Chair>().GetPlayerType() != PlayerType.AI)
                {
                    fObject.SetActive(true);
                    fText.text = "Spawn AI";
                }
                else
                {
                    fObject.SetActive(true);
                    fText.text = "Remove AI";
                }   
            }

            if (interactObject.GetComponent<Chair>().GetPlayerType() == PlayerType.None)
            {
                eObject.SetActive(true);
                eText.text = "Sit";
            }
            else
                eObject.SetActive(false);
        }
    }

    public void Hide()
    {
        eObject.SetActive(false);
        fObject.SetActive(false);
    }
}
