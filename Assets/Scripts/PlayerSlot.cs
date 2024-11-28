using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    private const int NUM_MODELS = 5;

    [SerializeField] private GameObject spawnAI;
    [SerializeField] private GameObject playerHeader;
    [SerializeField] private int slotNum;

    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button changeModelLeft;
    [SerializeField] private Button changeModelRight;

    private void Start()
    {
        ThirteenMultiplayer.Instance.OnPlayerDataNetworkListChanged += ThirteenMultiplayer_OnPlayerDataNetworkListChanged;
        LobbyManager.Instance.OnReadyChanged += LobbyManager_OnReadyChanged;

        changeModelLeft.onClick.AddListener(() =>
        {
            ChangeModel(-1);
        });
        changeModelRight.onClick.AddListener(() =>
        {
            ChangeModel(1);
        });

        UpdatePlayer();
    }

    private void LobbyManager_OnReadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void ThirteenMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (ThirteenMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();

            PlayerData playerData = ThirteenMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            if (NetworkManager.Singleton.LocalClientId == playerData.clientID)
            {
                changeModelLeft.gameObject.SetActive(true);
                changeModelRight.gameObject.SetActive(true);
            }
            else
            {
                changeModelLeft.gameObject.SetActive(false);
                changeModelRight.gameObject.SetActive(false);
            }
            playerVisual.LoadModel(playerData.modelNum);
            readyGameObject.SetActive(LobbyManager.Instance.IsPlayerReady(playerData.clientID));
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ChangeModel(int direction)
    {
        PlayerData playerData = ThirteenMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
        if (direction == -1)
            ThirteenMultiplayer.Instance.ChangeModelNum((playerData.modelNum % NUM_MODELS) + 1);
        else
            ThirteenMultiplayer.Instance.ChangeModelNum((playerData.modelNum - 2 + NUM_MODELS) % NUM_MODELS + 1);
    }

    public void SpawnAI()
    {
        if (LobbyManager.Instance.SpawnAI(slotNum))
        {
            spawnAI.SetActive(false);
            playerHeader.SetActive(true);
        }
    }

    public void RemoveAI()
    {
        if (LobbyManager.Instance.RemovePlayer(slotNum))
        {
            spawnAI.SetActive(true);
            playerHeader.SetActive(false);
        }
    }

    public void HumanSpawned() => spawnAI.SetActive(false);
    public void SlotOpened() => spawnAI.SetActive(true);
}
