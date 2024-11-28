using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public interface IInteractable
{
    public void Highlight(GameObject obj);
    public void Unhighlight();

    [ServerRpc(RequireOwnership = false)]
    public void InteractServerRpc(NetworkObjectReference obj);
}
