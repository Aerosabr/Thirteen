using NUnit.Framework;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class JoinGameUI : MonoBehaviour
{
    [SerializeField] private Button quickJoin;
    [SerializeField] private Button joinCode;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;

    private void Awake()
    {
        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        quickJoin.onClick.AddListener(() => {
            ThirteenLobby.Instance.QuickJoin();
        });

        joinCode.onClick.AddListener(() =>
        {
            ThirteenLobby.Instance.JoinWithCode(joinCodeInputField.text);
        });

        ThirteenLobby.Instance.OnLobbyListChanged += Lobby_OnLobbyListChanged;

        MenuManager.Instance.OnStateChanged += MenuManager_OnStateChanged;
        Hide();
    }

    private void Lobby_OnLobbyListChanged(object sender, ThirteenLobby.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void MenuManager_OnStateChanged(object sender, MenuManager.OnStateChangedEventArgs e)
    {
        if (e.state == MenuManager.MenuState.JoinGame)
            Show();
        else
            Hide();
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }

    private void OnDestroy()
    {
        ThirteenLobby.Instance.OnLobbyListChanged -= Lobby_OnLobbyListChanged;
    }

    private void Show() => gameObject.SetActive(true);
    private void Hide() => gameObject.SetActive(false);
}
