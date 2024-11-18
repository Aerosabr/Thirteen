using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerType playerType;
    protected Chair chair;
    protected int playerID;
    [SerializeField] protected PlayerVisual playerVisual;

    public virtual void InitializePlayer(int playerPos) => Debug.Log("Base Player InitializePlayer");
    public virtual void SitOnChair(Chair chair) => Debug.Log("Base Player SitOnChair");
}

public enum PlayerType
{
    AI,
    Player
}
