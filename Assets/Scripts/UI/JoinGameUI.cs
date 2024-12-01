using UnityEngine;

public class JoinGameUI : MonoBehaviour
{
    private void Start()
    {
        MenuManager.Instance.OnStateChanged += MenuManager_OnStateChanged;
        Hide();
    }

    private void MenuManager_OnStateChanged(object sender, MenuManager.OnStateChangedEventArgs e)
    {
        if (e.state == MenuManager.MenuState.JoinGame)
            Show();
        else
            Hide();
    }

    private void Show() => gameObject.SetActive(true);
    private void Hide() => gameObject.SetActive(false);
}
