using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Player player;

    public void CardThrown()
    {
        player.CardThrown();
    }

    public void PlayAnimation(string trigger) => anim.SetTrigger(trigger);
}
