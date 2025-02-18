using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    private void Start()
    {
        MenuManager.Instance.OnStateChanged += MenuManager_OnStateChanged;
    }

    private void MenuManager_OnStateChanged(object sender, MenuManager.OnStateChangedEventArgs e)
    {
        if (e.state == MenuManager.MenuState.MainMenu)
            Show();
        else
            Hide();
    }

    private void Show() => gameObject.SetActive(true);
    private void Hide() => gameObject.SetActive(false);
}
