using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public PlayerType playerType;
    protected Chair chair;
    protected int playerID;
    [SerializeField] protected PlayerVisual playerVisual;
    [SerializeField] protected PlayerInfo playerInfo;

    public virtual void SitOnChair(NetworkObjectReference chairRef) => Debug.Log("Base Player SitOnChair");
    public virtual void ExitChair() => Debug.Log("Base Player ExitChair");
    public virtual void CardThrown() => Debug.Log("Base Player CardThrown");
    public int GetPlayerID() => playerID;
    public PlayerInfo GetPlayerInfo() => playerInfo;
    public Chair GetChair() => chair;
}

public enum PlayerType
{
    AI,
    Player,
    None
}
