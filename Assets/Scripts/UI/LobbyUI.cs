using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [SerializeField] private Button startGameButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;


    private void Awake()
    {
        Instance = this;

        startGameButton.onClick.AddListener(() =>
        {
            
        });
        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            SceneLoader.Load(SceneLoader.Scene.MenuScene);
        });
        readyButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.SetPlayerReady();
        });
    }

    private void Start()
    {
        
    }

}
