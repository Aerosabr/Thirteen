using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [SerializeField] private Transform aiTemplate;
    [SerializeField] private Transform humanTemplate;

    [SerializeField] private List<PlayerSlot> playerSlots;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //SpawnHuman(1);
    }

    public bool SpawnHuman(int playerPos)
    {
        if (!PlayerManager.Instance.Players.ContainsKey(playerPos))
        {
            Transform player = Instantiate(humanTemplate, playerSlots[playerPos - 1].transform.position, playerSlots[playerPos - 1].transform.rotation, PlayerManager.Instance.transform);
            PlayerManager.Instance.Players.Add(playerPos, player.GetComponent<Player>());
            playerSlots[playerPos - 1].HumanSpawned();
            return true;
        }

        return false;
    }

    public bool SpawnAI(int playerPos)
    {
        if (!PlayerManager.Instance.Players.ContainsKey(playerPos))
        {
            Transform player = Instantiate(aiTemplate, playerSlots[playerPos - 1].transform.position, playerSlots[playerPos - 1].transform.rotation, PlayerManager.Instance.transform);
            PlayerManager.Instance.Players.Add(playerPos, player.GetComponent<Player>());
            return true;
        }

        return false;
    }

    public bool RemovePlayer(int playerPos)
    {
        if (PlayerManager.Instance.Players.ContainsKey(playerPos))
        {
            Destroy(PlayerManager.Instance.Players[playerPos].gameObject);
            PlayerManager.Instance.Players.Remove(playerPos);
            return true;
        }

        return false;
    }
}
