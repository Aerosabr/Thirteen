using UnityEngine;

public class PlayerVisualFollow : MonoBehaviour
{
    public Transform sourceObject;

    void LateUpdate()
    {
        if (sourceObject != null)
        {
            transform.position = sourceObject.localPosition;
        }
    }
}
