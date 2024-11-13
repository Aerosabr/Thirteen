using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void Highlight();
    public void Unhighlight();

    public void Interact(GameObject player);
}
