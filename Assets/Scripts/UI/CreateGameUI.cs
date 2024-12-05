using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateGameUI : MonoBehaviour
{
    [SerializeField] private Button startGame;
    [SerializeField] private TMP_InputField lobbyNameInput;

    private void Start()
    {
        startGame.onClick.AddListener(() => {
            ThirteenLobby.Instance.CreateLobby(lobbyNameInput.text, false);
        });

        MenuManager.Instance.OnStateChanged += MenuManager_OnStateChanged;
        Hide();
    }

    private void MenuManager_OnStateChanged(object sender, MenuManager.OnStateChangedEventArgs e)
    {
        if (e.state == MenuManager.MenuState.CreateGame)
            Show();
        else
            Hide();
    }

    private void Show() => gameObject.SetActive(true);
    private void Hide() => gameObject.SetActive(false);
}
