using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void EnableOutline();
    public void DisableOutline();

    public void Interact(GameObject player);
}
