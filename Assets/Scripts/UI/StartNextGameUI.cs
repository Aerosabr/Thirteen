using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartNextGameUI : MonoBehaviour
{
    public static StartNextGameUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI text;
    private List<Player> playersReady = new List<Player>();
    private bool awaitingReady;

    private void Awake()
    {
        Instance = this;
    }

    public void StartUI()
    {
        awaitingReady = true;
        text.gameObject.SetActive(true);
        text.text = $"Press SPACE To Start Next Game: {playersReady.Count}/{PlayerManager.Instance.GetNumHumans()}";
    }

    public void ReadyUp(Player player)
    {
        if (playersReady.Contains(player))
            playersReady.Remove(player);
        else
            playersReady.Add(player);

        text.text = $"Press SPACE To Start Next Game: {playersReady.Count}/{PlayerManager.Instance.GetNumHumans()}";

        if (playersReady.Count == PlayerManager.Instance.GetNumHumans())
        {
            Table.Instance.StartGame();
            awaitingReady = false;
            playersReady.Clear();
            text.gameObject.SetActive(false);
        }
    }

    public bool GetAwaitingReady() => awaitingReady;
}
