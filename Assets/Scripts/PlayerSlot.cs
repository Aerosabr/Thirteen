using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    [SerializeField] private GameObject spawnAI;
    [SerializeField] private GameObject playerHeader;
    [SerializeField] private int slotNum;

    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private PlayerVisual playerVisual;

    private void Start()
    {
        ThirteenMultiplayer.Instance.OnPlayerDataNetworkListChanged += ThirteenMultiplayer_OnPlayerDataNetworkListChanged;
        LobbyManager.Instance.OnReadyChanged += LobbyManager_OnReadyChanged;

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
