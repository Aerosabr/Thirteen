using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }

    [SerializeField] private GameObject Sit;
    [SerializeField] private GameObject SpawnAI;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(InteractableObject interactObject, bool isServer)
    {
        if (isServer)
        {
            SpawnAI.SetActive(true);
        }

        Sit.SetActive(true);
    }

    public void Hide()
    {
        Sit.SetActive(false);
        SpawnAI.SetActive(false);
    }
}
