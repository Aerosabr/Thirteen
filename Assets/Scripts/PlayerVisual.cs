using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private GameObject _mainCamera;
    [SerializeField] private GameObject head;

    void Start()
    {
        if (_mainCamera == null)
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

   
    void Update()
    {
        Vector3 rotation = new Vector3(_mainCamera.transform.rotation.x, _mainCamera.transform.rotation.y + 90, _mainCamera.transform.rotation.z - 90);
        //head.transform.rotation = Quaternion.Euler(rotation);
        head.transform.rotation = _mainCamera.transform.rotation;
    }
}
