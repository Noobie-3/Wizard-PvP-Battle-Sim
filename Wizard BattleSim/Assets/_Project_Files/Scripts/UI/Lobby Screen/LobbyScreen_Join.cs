using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;


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

    private async void JoinLobbyByCode()
    {
        if (LobbyCode == null)
        { 
            print("Lobby code is null");
        return;
        }
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

    public  void JoinLobbyByIndex(int index)
    {
        if (index < 0 || index >= AvaliableLobbies.Length)
        {
            Debug.LogError("Invalid lobby index!");
            return;
        }

        // Set the lobby code from the selected lobby
        LobbyCode = AvaliableLobbies[index].LobbyCode;

        // Call the method to join the lobby using the code
        JoinLobbyByCode();
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
