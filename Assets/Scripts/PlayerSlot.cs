using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    [SerializeField] private GameObject spawnAI;
    [SerializeField] private GameObject playerHeader;
    [SerializeField] private int slotNum;

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
