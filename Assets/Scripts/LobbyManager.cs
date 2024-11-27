using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [SerializeField] private Transform aiTemplate;
    [SerializeField] private Transform humanTemplate;

    [SerializeField] private List<PlayerSlot> playerSlots;

    public event EventHandler OnReadyChanged;
    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake()
    {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    void Start()
    {
        //SpawnHuman(1);
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            SceneLoader.LoadNetwork(SceneLoader.Scene.GameScene);
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        playerReadyDictionary[clientId] = true;

        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }



    public bool SpawnHuman(int playerPos)
    {
        if (!PlayerManager.Instance.Players.ContainsKey(playerPos))
        {
            Transform player = Instantiate(humanTemplate, playerSlots[playerPos - 1].transform.position, playerSlots[playerPos - 1].transform.rotation, PlayerManager.Instance.transform);
            PlayerManager.Instance.Players.Add(playerPos, player.GetComponent<Player>());
            playerSlots[playerPos - 1].HumanSpawned();
            return true;
        }

        return false;
    }

    public bool SpawnAI(int playerPos)
    {
        if (!PlayerManager.Instance.Players.ContainsKey(playerPos))
        {
            Transform player = Instantiate(aiTemplate, playerSlots[playerPos - 1].transform.position, playerSlots[playerPos - 1].transform.rotation, PlayerManager.Instance.transform);
            PlayerManager.Instance.Players.Add(playerPos, player.GetComponent<Player>());
            return true;
        }

        return false;
    }

    public bool RemovePlayer(int playerPos)
    {
        if (PlayerManager.Instance.Players.ContainsKey(playerPos))
        {
            Destroy(PlayerManager.Instance.Players[playerPos].gameObject);
            PlayerManager.Instance.Players.Remove(playerPos);
            return true;
        }

        return false;
    }
}
