using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AssetInventory;
public class LObbyScreen_create : MonoBehaviour
{

    [SerializeField] private Lobby HostLobby;
    [SerializeField] private Lobby JoinedLobby;
    [SerializeField] private float Timer;
    [SerializeField] private float HeatBeatInterval = 5.0f;
    [SerializeField] private string PlayerName;
    [SerializeField] private Map_DB MapDB;
    [SerializeField] private Map_SO SelectedMap;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        PlayerName = "Noob Test " + UnityEngine.Random.Range(10, 99);
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Player Name is : " + PlayerName);
    }

public async void Create_Lobby()
    {
        try
        {
            string LobbyName = " MyLobby";
            int MaxPlayers = 4;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerName) }
                    }
                },

                Data = new Dictionary<string, DataObject>
                {
                    { "Level", new DataObject(DataObject.VisibilityOptions.Public, LobbyName) }
                },

            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, MaxPlayers);
            HostLobby = lobby;
            JoinedLobby = lobby;
            Debug.Log("Lobby Created with " + lobby.Id + " and " + lobby.MaxPlayers + " players");
        }

        catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }

    }

    private async void HandleLobbyUpdate()
    {
        if(HostLobby == null)
        {
            return;
        }

        Timer -= Time.deltaTime;
        if(Timer <= 0)
        {
            Timer = HeatBeatInterval;
            await LobbyService.Instance.SendHeartbeatPingAsync(HostLobby.Id);
        }
    }

    private async void kickPlayer(string PlayerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(HostLobby.Id, PlayerId);
        }
        catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void UpdateLobbyLevel(string LevelName)
    {
        try
        {
            HostLobby = await LobbyService.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>
            {
                { "Level", new DataObject(DataObject.VisibilityOptions.Public, LevelName) }
            }
            }
            );
        }
        catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    

        

    }

    private async void migrateLobbyHost()
    {
        try
        {
            HostLobby = await LobbyService.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions()
            {
                HostId = JoinedLobby.Players[1].Id
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }

    }

    private void Update()   
    {
        HandleLobbyUpdate();
    }
}
