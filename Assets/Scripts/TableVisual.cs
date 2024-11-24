using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TableVisual : MonoBehaviour
{
    public float targetYRotation = 0f; // Target Y-axis rotation
    public float rotationSpeed = 100f; // Speed of rotation

    private void Start()
    {
        Table.Instance.OnPlayerTurn += Table_OnPlayerTurn;
    }

    private void Table_OnPlayerTurn(object sender, Table.OnPlayerTurnEventArgs e)
    {
        List<float> pointerRotation = new List<float>() { 0, 90, 180, 270, 0 };
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, pointerRotation[e.currentPlayer], transform.eulerAngles.z);
        targetYRotation = pointerRotation[e.currentPlayer];
    }

    
}
