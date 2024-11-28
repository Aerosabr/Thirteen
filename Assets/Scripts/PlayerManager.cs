using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private Transform playerPrefab;
    public Dictionary<int, Player> Players = new Dictionary<int, Player>();
    public int numPlayers = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
            InitializePlayersServerRpc();
    }

    [ServerRpc]
    private void InitializePlayersServerRpc()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log(clientId);
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            PlayerData playerData = ThirteenMultiplayer.Instance.GetPlayerDataFromPlayerIndex(ThirteenMultiplayer.Instance.GetPlayerDataIndexFromClientId(clientId));
            Players.Add(numPlayers + 1, playerTransform.GetComponent<Player>());
            numPlayers++;
            playerTransform.GetComponent<Player>().InitializePlayerServerRpc(numPlayers);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") 
        {
            Debug.Log("Target scene loaded! Calling specific function.");
            foreach (int key in Players.Keys)
            {
                Players[key].InitializePlayerServerRpc(key);
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
