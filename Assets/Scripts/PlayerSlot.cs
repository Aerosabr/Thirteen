using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    private const int NUM_MODELS = 5;

    [SerializeField] private GameObject playerHeader;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button changeModelLeft;
    [SerializeField] private Button changeModelRight;

    private void Start()
    {
        changeModelLeft.onClick.AddListener(() =>
        {
            ChangeModel(-1);
        });
        changeModelRight.onClick.AddListener(() =>
        {
            ChangeModel(1);
        });
    }

    private void ChangeModel(int direction)
    {
        /*
        PlayerData playerData = ThirteenMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
        if (direction == -1)
            ThirteenMultiplayer.Instance.ChangeModelNum((playerData.modelNum % NUM_MODELS) + 1);
        else
            ThirteenMultiplayer.Instance.ChangeModelNum((playerData.modelNum - 2 + NUM_MODELS) % NUM_MODELS + 1);
        */
    }
}
