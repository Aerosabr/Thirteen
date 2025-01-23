using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateGameUI : MonoBehaviour
{
    [SerializeField] private Button createPublic;
    [SerializeField] private Button createPrivate;
    [SerializeField] private TMP_InputField lobbyNameInput;

    private void Start()
    {
        createPublic.onClick.AddListener(() => {
            ThirteenLobby.Instance.CreateLobby(lobbyNameInput.text, false);
        });

        createPrivate.onClick.AddListener(() => {
            ThirteenLobby.Instance.CreateLobby(lobbyNameInput.text, true);
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
