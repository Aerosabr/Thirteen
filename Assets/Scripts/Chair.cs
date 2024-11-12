using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : MonoBehaviour, IInteractable
{
    [SerializeField] private Outline outline;
    [SerializeField] private GameObject sitPoint;

    public void EnableOutline()
    {
        outline.enabled = true;
    }

    public void DisableOutline()
    {
        outline.enabled = false;
    }

    public void Interact(GameObject player)
    {
        player.GetComponent<Player>().SetPosition(sitPoint.transform.position);
    }

}
