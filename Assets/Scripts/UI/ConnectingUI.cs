using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    private void Start()
    {
        ThirteenMultiplayer.Instance.OnTryingToJoinGame += ThirteenMultiplayer_OnTryingToJoinGame;
        ThirteenMultiplayer.Instance.OnFailedToJoinGame += ThirteenMultiplayer_OnFailedToJoinGame;
        Hide();
    }

    private void ThirteenMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void ThirteenMultiplayer_OnTryingToJoinGame(object sender, System.EventArgs e)
    {
        Show();
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
        ThirteenMultiplayer.Instance.OnTryingToJoinGame -= ThirteenMultiplayer_OnTryingToJoinGame;
        ThirteenMultiplayer.Instance.OnFailedToJoinGame -= ThirteenMultiplayer_OnFailedToJoinGame;
    }
}
