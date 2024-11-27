using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;

    private void Awake()
    {
        createGameButton.onClick.AddListener(() =>
        {
            ThirteenMultiplayer.Instance.StartHost();
            SceneLoader.LoadNetwork(SceneLoader.Scene.LobbyScene);
        });
        joinGameButton.onClick.AddListener(() =>
        {
            ThirteenMultiplayer.Instance.StartClient();
            
        });
    }
}
