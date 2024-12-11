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
    [SerializeField] private Transform aiPrefab;
    private NetworkList<PlayerInfo> Players;
    public int numPlayers = 0;

    private void Awake()
    {
        Instance = this;
        Players = new NetworkList<PlayerInfo>();
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
    public void SpawnPlayerServerRpc(ulong clientId, PlayerInfo playerInfo)
    {
        Debug.Log("Spawn player: " + clientId);
        Players.Add(playerInfo);
        numPlayers++;
        Transform playerTransform = Instantiate(playerPrefab);
        playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
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
    public PlayerInfo GetPlayerInfoFromID(ulong clientId)
    {
        foreach (PlayerInfo playerData in Players)
        {
            if (playerData.clientId == clientId)
                return playerData;
        }
        return default;
    }

    public Transform GetAIPrefab() => aiPrefab;
}
