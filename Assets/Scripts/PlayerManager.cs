using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public Dictionary<int, Player> Players = new Dictionary<int, Player>();

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") 
        {
            Debug.Log("Target scene loaded! Calling specific function.");
            foreach (int key in Players.Keys)
            {
                Players[key].InitializePlayer(key);
            }
        }
    }

    public int GetNumHumans()
    {
        int numHumans = 0;

        foreach (Player player in Players.Values)
        {
            if (player.playerType == PlayerType.Player)
                numHumans++;
        }

        return numHumans;
    }
}
