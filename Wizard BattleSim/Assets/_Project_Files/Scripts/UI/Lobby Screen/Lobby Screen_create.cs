using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AssetInventory;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
public class LObbyScreen_create : MonoBehaviour
{

    [SerializeField] private Lobby HostLobby;
    [SerializeField] private Lobby JoinedLobby;
    [SerializeField] private string LobbyName;
    [SerializeField] private TMP_InputField LobbyNameInput; 
    [SerializeField] private float Timer;
    [SerializeField] private float HeatBeatInterval = 5.0f;
    [SerializeField] private string PlayerName;
    [SerializeField] private Map_DB MapDB;
    [SerializeField] private Image MapImage;
    [SerializeField] private TextMeshProUGUI MapNameDisplay;
    [SerializeField] private int SelectedMap;
    [SerializeField] private int MaxPlayers;
    [SerializeField] private TextMeshProUGUI MaxPlayerCountText;
    [SerializeField] private TextMeshProUGUI LobbyCodeDisplay;
    [SerializeField] private string relayCode;
    [SerializeField] private TMP_InputField NameField;
    [SerializeField] private TextMeshProUGUI ErrorLogger;
    [SerializeField] private LobbyScreenSelector lobbyScreenSelector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        if (gameController.GC.connectionType != gameController.ConnectionType.Online)
        {
            lobbyScreenSelector.ChangeToNonOnlineLobby();
            return;
        }
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Player Name is : " + PlayerName);
        MapImage.sprite = MapDB.GetMapById(SelectedMap).Icon;
        MapNameDisplay.text = MapDB.GetMapById(SelectedMap).DisplayName;

    }

    [ConsoleCommand("Create a relay with 2 slots")]
    public async Task<string> CreateRelay()
    {
        try
        {
            //create a relay with 2 slots
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Relay created with join code: " + joinCode);

            Debug.Log("Join code: " + joinCode);
            relayCode = joinCode;


            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );

            NetworkManager.Singleton.StartHost();
            return joinCode;
        }
        catch (Unity.Services.Relay.RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }


    public async void Create_Lobby()
    {
        try
        {
            if (string.IsNullOrEmpty(LobbyName))
            {
                ShowError("Lobby Name is empty");
                print("Lobby Name Is Empty");
                return;
            }

            if (string.IsNullOrEmpty(PlayerName))
            {
                ShowError("Player Name is empty");
                print("Player Name Is Empty");
                return;
            }
            // ✅ Await relay creation and get the code here
            string relayCode = await CreateRelay();

            if (string.IsNullOrEmpty(relayCode))
            {
                ShowError("Failed to create relay");
                return;
            }

            Debug.Log("Relay code used in lobby: " + relayCode);

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
                { "Level", new DataObject(DataObject.VisibilityOptions.Public, SelectedMap.ToString()) },
                { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) }
            },
            };
            print("player name is " + PlayerName + " and relay code is " + relayCode + " and level is " + SelectedMap.ToString() + " and max players is " + MaxPlayers.ToString());

            if (MaxPlayers <= 0) MaxPlayers = 2;
            if (string.IsNullOrEmpty(LobbyName)) LobbyName = "New Lobby";

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, MaxPlayers, createLobbyOptions);

            HostLobby = lobby;
            JoinedLobby = lobby;
            ServerManager.Instance.Lobby = lobby;

            foreach(var player in ServerManager.Instance.Lobby.Players)
            {
                Debug.Log("Player ID: " + player.Id);
            }


            var OldState = PlayerStateManager.Singleton.LookupState(NetworkManager.Singleton.LocalClientId);

            var State = new CharacterSelectState(
                clientId: NetworkManager.Singleton.LocalClientId,
                characterId: OldState.CharacterId,
                wandID: OldState.WandID,
                spell0: OldState.Spell0,
                spell1: OldState.Spell1,
                spell2: OldState.Spell2,
                isLockedIn: OldState.IsLockedIn,
                //current players id
                playerLobbyId: AuthenticationService.Instance.PlayerId
                //player name
                , PlayerName: PlayerName
            );

            PlayerStateManager.Singleton.RequestAddOrUpdateStateServerRpc(State);
            lobbyScreenSelector.BackToMainScreen();
            LobbyCodeDisplay.text = "Lobby Code: " + lobby.LobbyCode;
            Debug.Log("Lobby Created with code: " + lobby.LobbyCode);
        }
        catch (LobbyServiceException e)
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

    public async void kickPlayer(string PlayerId)
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

    public async void UpdateLobbyLevel(int Level)
    {
        try
        {
            HostLobby = await LobbyService.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>
            {
                { "Level", new DataObject(DataObject.VisibilityOptions.Public, Level.ToString()) }
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

    public void ChangeLobbyName()
    {
        LobbyName = LobbyNameInput.text;
    }

    public void ChangeMapUp()
    {
        if(SelectedMap == MapDB.GetAllMaps().Length - 1)
        {
            return;
        }   
        SelectedMap += 1;
        if(MapDB.GetMapById(SelectedMap) == null)
        {
            SelectedMap -= 1;
            print("Map not found");
            return;
        }

        MapImage.sprite = MapDB.GetMapById(SelectedMap).Icon;
        MapNameDisplay.text = MapDB.GetMapById(SelectedMap).DisplayName;
    }

    public void ChangeMapDown()
    {
        if(SelectedMap == 0)
        {
            return;
        }
        SelectedMap -= 1;
        if (MapDB.GetMapById(SelectedMap) == null)
        {
            SelectedMap += 1;
            print("Map not found");
            return;
        }
        MapImage.sprite = MapDB.GetMapById(SelectedMap).Icon;
        MapNameDisplay.text = MapDB.GetMapById(SelectedMap).DisplayName;
    }

    public void increasePlayercount()
    {
        if(MaxPlayers == 10)
        {
            return;
        }
        MaxPlayers = MaxPlayers += 1;
        MaxPlayerCountText.text = "Max Players: " + MaxPlayers;
    }
    public void decreasePlayercount()
    {
        if(MaxPlayers == 1)
        {
            return;
        }
        MaxPlayers = MaxPlayers -= 1;
        
    }
    private void Update()   
    {
        HandleLobbyUpdate();
        MaxPlayerCountText.text = "Max Players: " + MaxPlayers;
    }

/*    public async void UpdatePlayerName()
    {
        try
        {
            NameChange();
            await LobbyService.Instance.UpdatePlayerAsync(HostLobby.ToString(), AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName) }
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }*/

    public void NameChange()
    {
        if (NameField == null)
            return;

        if (NameField.text != "")
            PlayerName = NameField.text;

    }

    private void ShowError(string error)
    {
        ErrorLogger.text = error;
        ErrorLogger.enabled = true;
        ErrorLogger.GetComponent<Animation>().Play();
    }





}
