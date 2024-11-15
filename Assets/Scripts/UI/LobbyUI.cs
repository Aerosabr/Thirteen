using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [SerializeField] private Button startGame;

    private void Awake()
    {
        Instance = this;

        startGame.onClick.AddListener(() =>
        {
            StartGame();
        });
    }

    private void Start()
    {
        
    }

    private void StartGame()
    {
        SceneLoader.Load(SceneLoader.Scene.GameScene);
    }
}
