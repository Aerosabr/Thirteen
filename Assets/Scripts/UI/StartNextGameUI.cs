using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class StartNextGameUI : NetworkBehaviour
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

    [ServerRpc(RequireOwnership = false)]
    public void UpdateUIServerRpc()
    {
        if (Table.Instance.GetNumPlayersAtTable() != 4)
        {
            UpdateUIClientRpc($"Waiting for more players: {Table.Instance.GetNumPlayersAtTable()}/4");
            return;
        }
        else if (Table.Instance.GetNumHumansReady() == Table.Instance.GetNumHumansAtTable())
        {
            HideClientRpc();
            return;
        }
        UpdateUIClientRpc($"Press SPACE To Start Next Game: {Table.Instance.GetNumHumansReady()}/{Table.Instance.GetNumHumansAtTable()}");
    }

    [ClientRpc]
    private void UpdateUIClientRpc(string uiText)
    {
        text.text = uiText;
    }

    [ClientRpc]
    private void HideClientRpc() => Hide();

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
