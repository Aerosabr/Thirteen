using System;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public MenuState state;
    }

    public enum MenuState
    {
        MainMenu,
        CreateGame,
        JoinGame,
        Options,
        None,
    }

    private MenuState menuState = MenuState.MainMenu;
    [SerializeField] private MainMenuCamera menuCamera;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
    }

    public void ChangeMenuState(string state)
    {
        MenuState newState = (MenuState)Enum.Parse(typeof(MenuState), state);
        switch (newState)
        {
            case MenuState.MainMenu:
                if (menuState == MenuState.CreateGame || menuState == MenuState.JoinGame)
                {
                    menuState = MenuState.None;
                    ChangeUI();
                    menuState = newState;
                    menuCamera.PlayCameraTransition("Camera2");
                }
                else
                {
                    menuState = newState;
                    ChangeUI();
                }
                break;

            case MenuState.CreateGame:
            case MenuState.JoinGame:
                menuState = MenuState.None;
                ChangeUI();
                menuState = newState;
                menuCamera.PlayCameraTransition("Camera1");
                break;

            case MenuState.Options:
                menuState = newState;
                ChangeUI();
                break;
        }
    }

    public void ChangeUI()
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
        {
            state = menuState
        });
    }
}
