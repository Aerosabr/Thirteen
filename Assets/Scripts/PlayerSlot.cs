using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    private const int NUM_MODELS = 5;

    private int modelNum;
    [SerializeField] private GameObject playerHeader;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button changeModelLeft;
    [SerializeField] private Button changeModelRight;
    [SerializeField] private TMP_InputField playerNameInput;

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

        playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
        ChangeModelNum(Random.Range(0, NUM_MODELS + 1));
    }

    private void OnPlayerNameChanged(string newName)
    {
        PlayerData.Instance.playerInfo.playerName = newName;
    }

    private void ChangeModelNum(int newModelNum)
    {
        modelNum = newModelNum;
        playerVisual.LoadModel(modelNum);
        PlayerData.Instance.playerInfo.modelNum = newModelNum;
    }

    private void ChangeModel(int direction)
    {
        if (direction == -1)
            ChangeModelNum((modelNum % NUM_MODELS) + 1);
        else
            ChangeModelNum((modelNum - 2 + NUM_MODELS) % NUM_MODELS + 1);
    }
}
