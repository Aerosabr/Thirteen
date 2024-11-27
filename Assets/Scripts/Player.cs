using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public PlayerType playerType;
    protected Chair chair;
    protected int playerID;
    [SerializeField] protected PlayerVisual playerVisual;

    public virtual void InitializePlayer(int playerPos) => Debug.Log("Base Player InitializePlayer");
    public virtual void SitOnChair(Chair chair) => Debug.Log("Base Player SitOnChair");
    public virtual void CardThrown() => Debug.Log("Base Player CardThrown");
    public int GetPlayerID() => playerID;
}

public enum PlayerType
{
    AI,
    Player
}
