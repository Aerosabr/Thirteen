using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionResponseUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        ThirteenMultiplayer.Instance.OnFailedToJoinGame += ThirteenMultiplayer_OnFailedToJoinGame;

        Hide();
    }

    private void ThirteenMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        Show();

        messageText.text = NetworkManager.Singleton.DisconnectReason;

        if (messageText.text == "")
            messageText.text = "Failed to connect"; 
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        ThirteenMultiplayer.Instance.OnFailedToJoinGame -= ThirteenMultiplayer_OnFailedToJoinGame;
    }
}
