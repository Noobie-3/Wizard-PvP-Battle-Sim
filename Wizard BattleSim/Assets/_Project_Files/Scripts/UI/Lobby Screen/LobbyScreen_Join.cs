using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class LobbyScreen_Join : MonoBehaviour
{

    [SerializeField] private string PlayerName;
    [SerializeField] private Lobby joinedLobby;
    [SerializeField] private float Timer;
    [SerializeField] private TMP_InputField NameField;
    [SerializeField] private TMP_InputField LobbyCodeField;
    [SerializeField] private string LobbyCode;
    [SerializeField] private GameObject[] LobbyObjects;
    [SerializeField] private Map_DB MapDB;
    [SerializeField] private Map_SO SelectedMap;
    [SerializeField] private Lobby[] AvaliableLobbies;
    [SerializeField] private TextMeshProUGUI ErrorLogger;
    [SerializeField] private LobbyScreenSelector lobbyScreenSelector;
    [SerializeField] private ServerManager serverManager;


    private void Start()
    {
        PlayerName = "Mage" + UnityEngine.Random.Range(10, 99);
        Timer = 0;
        UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        };
        AuthenticationService.Instance.SignInAnonymouslyAsync();
        ListLobbies();
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scene.name == gameController.GC.CharacterSelectSceneName)
            {
                OnsceneLoaded();
            }
        };
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"Successfully connected as client {clientId}");

            // Now it's safe to setup your CharacterSelectState

            CharacterSelectState OldState = PlayerStateManager.Singleton.LookupState(NetworkManager.Singleton.LocalClientId);

            print("old state info: ClientID:" + OldState.ClientId + "CharacterID:" + OldState.CharacterId + "PlayerLobbyId:" + OldState.PlayerLobbyId);

            var State = new CharacterSelectState(
                clientId: NetworkManager.Singleton.LocalClientId,
                characterId: OldState.CharacterId,
                wandID: OldState.WandID,
                spell0: OldState.Spell0,
                spell1: OldState.Spell1,
                spell2: OldState.Spell2,
                isLockedIn: OldState.IsLockedIn,
                //current players id
                playerLobbyId: AuthenticationService.Instance.PlayerId,
                //player name
                PlayerName: PlayerName
                
            );
            if (PlayerStateManager.Singleton == null)
            {
                Debug.LogError("PlayerStateManager is null");
                ShowError("PlayerStateManager is null");
                return;
            }
            if (PlayerStateManager.Singleton == null)
            {
                Debug.LogError("PlayerStateManager is null");
                ShowError("PlayerStateManager is null");
                return;
            }

            PlayerStateManager.Singleton.RequestAddOrUpdateStateServerRpc(State);

        }
    }

    public async void ListLobbies()
    {
        try
        {
            // Set lobby query options
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 10, // Number of lobbies to fetch
                Filters = new List<QueryFilter>
            {
                // Only show lobbies with open slots
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            },
                // Order by creation time (newest first)
                Order = new List<QueryOrder> { new QueryOrder(false, QueryOrder.FieldOptions.Created) }
            };

            // Fetch available lobbies
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
            Debug.Log("Lobbies found: " + queryResponse.Results.Count);

            // Ensure we don't display outdated lobbies
            AvaliableLobbies = queryResponse.Results.ToArray();
            print(AvaliableLobbies.Length + " Lobbies found");
            // Loop through each lobby and update the UI
            for (int i = 0; i < queryResponse.Results.Count && i < LobbyObjects.Length; i++)
            {
                var lobby = queryResponse.Results[i];

                // Activate the lobby slot (if inactive)
                LobbyObjects[i].SetActive(true);

                // Update lobby name and player count
                LobbyObjects[i].transform.Find("Lobby Name").GetComponent<TextMeshProUGUI>().text = lobby.Name;
                LobbyObjects[i].transform.Find("Lobby Size").GetComponent<TextMeshProUGUI>().text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

                // Update map image (if MapDB and SelectedMap are available)
                if (MapDB != null && SelectedMap != null)
                {
                    var mapImage = LobbyObjects[i].transform.Find("Map Image").GetComponent<Image>();
                    mapImage.sprite = MapDB.GetMapById(SelectedMap.Id)?.Icon;
                }

                // Cache the index to prevent closure issues
                int lobbyIndex = i;

                // Set up the Join button for this lobby
                Button joinButton = LobbyObjects[i].transform.GetComponent<Button>();

                joinButton.onClick.RemoveAllListeners(); // Avoid stacking listeners
                joinButton.onClick.AddListener(() => JoinLobbyByIndex(lobbyIndex));

                Debug.Log("Lobby " + i + ": " + lobby.Name + " with " + lobby.Players.Count + " players");
            }

            // Hide any unused lobby slots
            for (int i = queryResponse.Results.Count; i < LobbyObjects.Length; i++)
            {
                LobbyObjects[i].SetActive(false);
            }

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Error fetching lobbies: " + e);
        }
    }

    [ConsoleCommand("Join a relay with a join code")]
    //join a relay
    public async Task<bool> JoinRelay(string joincode)
    {
        try
        {
            //join the relay with the join code provided
            Debug.Log("Joining relay with join code: " + joincode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joincode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();
            Debug.Log("Joined relay with join code: " + joincode);

            return true;
        }


        catch (RelayServiceException e)
        {//catch any exceptions
            Debug.Log(e);
            return false;
        }
    }


    public async void JoinLobbyByCode()
    {
        if (LobbyCode == "")
        { 
            ShowError( "Lobby code is empty! Please enter a valid code.");
            return;
        }
        else
        {
            print("Lobby code is: " + LobbyCode);
        }
        try
        {
            JoinLobbyByCodeOptions joinLobbyOptoins = new JoinLobbyByCodeOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName) },
                    }
                },
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(LobbyCode, joinLobbyOptoins);
            joinedLobby = lobby;
            Debug.Log("Joined lobby: " + lobby.Name + " with " + lobby.MaxPlayers + " players" + "using code: " + LobbyCode);

            
            foreach(var player in lobby.Players)
            {
                print("Player: " + player.Data["PlayerName"].Value + "player id: " + player.Id);
            }
             bool JoinedRelay = await JoinRelay(lobby.Data["RelayCode"].Value);
            if (JoinedRelay == false) {
                ShowError("Failed to join relay. Please try again." );
                print("Failed to join relay. Please try again." );
                return;
            }

            CharacterSelectState OldState = PlayerStateManager.Singleton.LookupState(NetworkManager.Singleton.LocalClientId);

            print("old state info: ClientID:" + OldState.ClientId + "CharacterID:" + OldState.CharacterId + "PlayerLobbyId:" + OldState.PlayerLobbyId);

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
            );

            PlayerStateManager.Singleton.RequestAddOrUpdateStateServerRpc(State);
            ServerManager.Instance.Lobby = lobby;
            lobbyScreenSelector.BackToMainScreen();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"{LobbyCode}: was not a valid code Please Try Again :>");
            Debug.LogError(e);
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby == null)
        {
            return;
        }

        Timer -= Time.deltaTime;
        if (Timer <= 0)
        {
            float LobbyUpdateInterval = 1.1f;
            Timer = LobbyUpdateInterval;
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            joinedLobby = lobby;
        }
    }

    public async void JoinLobbyByIndex(int index)
    {
        if (string.IsNullOrEmpty(PlayerName))
        {
            ShowError("Player name is empty! Please enter a valid name." );
            return;
        }

        print("PLayer Name Is " + PlayerName);

        print(index + " Index");
        if (index < 0 || index >= AvaliableLobbies.Length)
        {
            ShowError("Lobby is not available!");
            return;
        }
        JoinLobbyByIdOptions JoinLobbyOptions = new JoinLobbyByIdOptions
        {
            Player = new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerName) },
                    }
            },
        };

        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(AvaliableLobbies[index].Id, JoinLobbyOptions);


        bool joinedRelay = await JoinRelay(joinedLobby.Data["RelayCode"].Value);

        
        if(joinedRelay == false)
        {
            ShowError("Failed to join relay. Please try again." );
            print("Failed to join relay. Please try again." );
            return;
        }
        ServerManager.Instance.Lobby = joinedLobby;
        print("joined Relay bool is true");

        print("JOined Lobby: " + joinedLobby.Name + " with " + joinedLobby.MaxPlayers + " players");
    }

    public async void QuickJoinLobby()
    {
        
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();

        }
        catch (LobbyServiceException e)
        {
            ShowError("Quick join failed: " );
            Debug.LogError(e);
        }
    }

    public async void UpdatePlayerName()
    {
        try
        {
            NameChange();
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
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
    }   

    public async void LeaveLobby()
    {
        try
        {
            if (joinedLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                joinedLobby = null;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private void ShowError(string error)
    {
        ErrorLogger.text = error;
        ErrorLogger.enabled = true;
        ErrorLogger.GetComponent<Animation>().Play();
    }

    
    private void Update()
    {
        HandleLobbyPollForUpdates();
    }

    public void NameChange()
    {
        if(NameField == null )
            return;

        if(NameField.text != "")
            PlayerName = NameField.text;
    }
    public void LobbyCodeChange()
    {
        if(LobbyCodeField == null )
            return;

        if(LobbyCodeField.text != "")
            LobbyCode = LobbyCodeField.text;
    }

    public void OnsceneLoaded()
    {
        //check if already in a lobby if so move to the charcter select screen
        if (ServerManager.Instance.Lobby != null)
        {
            lobbyScreenSelector.BackToMainScreen();
            return;
        }
    }

    
}
