using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Player player;
    [SerializeField] private List<PlayerModels> models;

    private int modelNum = 0;

    public void LoadModel(int num)
    {
        foreach (GameObject bodyPart in models[modelNum].bodyParts)
        {
            bodyPart.SetActive(false);
        }

        modelNum = num;

        foreach (GameObject bodyPart in models[modelNum].bodyParts)
        {
            bodyPart.SetActive(true);
        }
    }

    public void CardThrown()
    {
        player.CardThrown();
    }

    public void PlayAnimation(string trigger)
    {
        anim.SetTrigger(trigger);
    }
}

[System.Serializable]
public struct PlayerModels
{
    public List<GameObject> bodyParts;
}
