using UnityEngine;
using UnityEngine.UI;

public class JoinGameUI : MonoBehaviour
{
    [SerializeField] private Button joinGame;

    private void Start()
    {
        joinGame.onClick.AddListener(() => {
            ThirteenLobby.Instance.QuickJoin();
        });

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
