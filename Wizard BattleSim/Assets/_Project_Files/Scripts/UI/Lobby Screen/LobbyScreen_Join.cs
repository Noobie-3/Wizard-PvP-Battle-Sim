using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;


public class LobbyScreen_Join : MonoBehaviour
{

    [SerializeField] private string PlayerName;
    [SerializeField] private Lobby joinedLobby;
    [SerializeField] private float Timer;
    [SerializeField] private InputField NameField;
    [SerializeField] private InputField LobbyCodeField;
    [SerializeField] private string LobbyCode;
    [SerializeField] private GameObject[] LobbyObjects;
    [SerializeField] private Map_DB MapDB;
    [SerializeField] private Map_SO SelectedMap;
    [SerializeField] private Lobby[] AvaliableLobbies;
    async void Start()
    {

            PlayerName = "Mage" + UnityEngine.Random.Range(10, 99);
        

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 10,
                Filters = new List<QueryFilter> { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder> { new QueryOrder(false, QueryOrder.FieldOptions.Created) }

            };
            
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            for(int i = 0; i < queryResponse.Results.Count; i++)
            {
                Debug.Log("Lobby: " + queryResponse.Results[i].Name + " with " + queryResponse.Results[i].MaxPlayers + " players");
                LobbyObjects[i].SetActive(true);
                LobbyObjects[i].transform.Find("Name").GetComponent<TextMeshProUGUI>().text = queryResponse.Results[i].Name;
                LobbyObjects[i].transform.Find("Lobby Size").GetComponent<TextMeshProUGUI>().text = queryResponse.Results[i].Players.Count + "/" + queryResponse.Results[i].MaxPlayers;
                if(MapDB == null)
                    return;
                LobbyObjects[i].transform.Find("Map Image").GetComponent<Image>().sprite = MapDB.GetMapById(SelectedMap.Id).Icon;
            }
            AvaliableLobbies = queryResponse.Results.ToArray();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void JoinLobbyByCode()
    {
        if (LobbyCode == null)
            print("Lobby code is null");
            return;
        try
        {
            JoinLobbyByCodeOptions createLobbyOptions = new JoinLobbyByCodeOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName) },
                    }
                },
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(LobbyCode);
            joinedLobby = lobby;
            Debug.Log("Joined lobby: " + lobby.Name + " with " + lobby.MaxPlayers + " players" + "using code: " + LobbyCode);

            
            foreach(var player in lobby.Players)
            {
                print("Player: " + player.Data["PlayerName"].Value + "player id: " + player.Id);
            }
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

    public  void JoinLobbyByButton()
    {
        try
        {
            LobbyCode= AvaliableLobbies[0].LobbyCode;
            JoinLobbyByCode();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void QuickJoinLobby()
    {
        
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void UpdatePlayerName()
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

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
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

    
}
