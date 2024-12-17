using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerData : NetworkBehaviour
{
    public static PlayerData Instance { get; private set; }
    public PlayerInfo playerInfo;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        playerInfo.playerName = "Player";
    }

    public void SpawnCharacter(ulong clientId)
    {
        playerInfo.clientId = clientId;
        Debug.Log(playerInfo.playerName);
        PlayerManager.Instance.SpawnPlayerServerRpc(clientId, playerInfo);
    }
}

[System.Serializable]
public struct PlayerInfo : IEquatable<PlayerInfo>, INetworkSerializable
{
    public ulong clientId;
    public FixedString64Bytes playerName;
    public int modelNum;


    public bool Equals(PlayerInfo other)
    {
        return
            clientId == other.clientId &&
            playerName == other.playerName &&
            modelNum == other.modelNum;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref modelNum);
    }

}
