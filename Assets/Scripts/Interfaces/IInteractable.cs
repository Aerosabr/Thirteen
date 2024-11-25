using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void Highlight(GameObject obj);
    public void Unhighlight();

    public void Interact(GameObject obj);
}
