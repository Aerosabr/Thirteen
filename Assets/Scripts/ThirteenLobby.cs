using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class ThirteenLobby : MonoBehaviour
{
    public static ThirteenLobby Instance { get; private set; }

    private Lobby joinedLobby;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(this);

        InitializeUnityAuthentication();
    }

    private async void InitializeUnityAuthentication()
    {
        if(UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(Random.Range(0, 10000).ToString());
            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, ThirteenMultiplayer.MAX_PLAYER_AMOUNT, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
            });

            ThirteenMultiplayer.Instance.StartHost();
            SceneLoader.LoadNetwork(SceneLoader.Scene.GameScene2);
        } 
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void QuickJoin()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            ThirteenMultiplayer.Instance.StartClient();
        } 
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
