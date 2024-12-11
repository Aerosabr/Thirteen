using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public abstract class InteractableObject : NetworkBehaviour
{
    public enum InteractType
    { 
        Card,
        Chair
    }

    protected InteractType interactType;

    public abstract bool Highlight(GameObject obj);
    public abstract void Unhighlight();

    public InteractType GetInteractType() => interactType;

    public abstract void Interact(NetworkObjectReference obj);
}
