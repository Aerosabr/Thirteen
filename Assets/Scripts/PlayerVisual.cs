using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Player player;
    [SerializeField] private List<PlayerModels> models;

    private void Awake()
    {
        foreach (GameObject bodyPart in models[Random.Range(0, models.Count)].bodyParts)
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
        Debug.Log(player.gameObject.name + ": " + trigger);
        anim.SetTrigger(trigger);
    }
}

[System.Serializable]
public struct PlayerModels
{
    public List<GameObject> bodyParts;
}
