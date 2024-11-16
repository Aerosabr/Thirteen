using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Playables;

public class AI : Player
{
    public override void SitOnChair(Chair chair)
    {
        transform.position = chair.GetSitPoint().transform.position;
        transform.rotation = chair.GetSitPoint().transform.rotation;
        this.chair = chair;
    }

    public override void InitializePlayer(int playerPos)
    {
        Table.Instance.GetChair(playerPos).Interact(gameObject);
    }
}
