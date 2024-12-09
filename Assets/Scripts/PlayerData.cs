using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerData : NetworkBehaviour
{
    public static PlayerData Instance { get; private set; }
    public string playerName;
    public int modelNum;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(this);
    }

    public void SpawnCharacter(ulong clientId)
    {
        PlayerManager.Instance.SpawnPlayerServerRpc(clientId, playerName, modelNum);
    }
}
