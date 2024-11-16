using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Animator anim;

    public void PlayAnimation(string trigger) => anim.SetTrigger(trigger);
}
