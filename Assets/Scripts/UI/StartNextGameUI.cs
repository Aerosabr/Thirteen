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

    public void UpdateUI()
    {
        Debug.Log(Table.Instance.GetNumPlayersAtTable() + "/4");
        if (Table.Instance.GetNumPlayersAtTable() != 4)
        {
            text.text = $"Waiting for more players: {Table.Instance.GetNumPlayersAtTable()}/4";
            return;
        }
        else if (Table.Instance.GetNumHumansReady() == Table.Instance.GetNumHumansAtTable())
        {
            Hide();
            return;
        }
        text.text = $"Press SPACE To Start Next Game: {Table.Instance.GetNumHumansReady()}/{Table.Instance.GetNumHumansAtTable()}";
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
