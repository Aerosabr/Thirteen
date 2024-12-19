using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartNextGameUI : MonoBehaviour
{
    public static StartNextGameUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI text;
    private List<Player> playersReady = new List<Player>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Hide();
    }

    public void UpdateUI(int playersReady, int maxPlayers)
    {
        Debug.Log(playersReady + " " + maxPlayers);
        if (playersReady == maxPlayers)
        {
            Hide();
            return;
        }

        text.text = $"Press SPACE To Start Next Game: {playersReady}/{maxPlayers}";
        Show();
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
