using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnectedServerRpc;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            PlayerData.Instance.SpawnCharacter(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerConnectedServerRpc(ulong clientId) => PlayerConnectedClientRpc(clientId);

    [ClientRpc]
    private void PlayerConnectedClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
            PlayerData.Instance.SpawnCharacter(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId, string playerName, int modelNum)
    {
        if (playerPrefab != null)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            Players.Add(numPlayers + 1, playerTransform.GetComponent<Player>());
            numPlayers++;
            playerTransform.GetComponent<Player>().InitializePlayerServerRpc(playerName, modelNum);
        }
        else
        {
            Debug.LogError("Player Prefab is not assigned!");
        }
    }

    /*
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene2") 
        {
            Debug.Log("Target scene loaded! Calling specific function.");
            foreach (int key in Players.Keys)
            {
                Players[key].InitializePlayerServerRpc(key);
            }
        }
    }
    */
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
