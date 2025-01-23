using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinGameUI : MonoBehaviour
{
    [SerializeField] private Button quickJoin;
    [SerializeField] private Button joinCode;
    [SerializeField] private TMP_InputField joinCodeInputField;

    private void Start()
    {
        quickJoin.onClick.AddListener(() => {
            ThirteenLobby.Instance.QuickJoin();
        });

        joinCode.onClick.AddListener(() =>
        {
            ThirteenLobby.Instance.JoinWithCode(joinCodeInputField.text);
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
