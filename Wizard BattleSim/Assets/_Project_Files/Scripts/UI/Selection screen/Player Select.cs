using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSelect : NetworkBehaviour
{
    private NetworkList<CharacterSelectState> Players;
    [SerializeField] private GameObject characterInfoPanel = null;
    [SerializeField] private TMPro.TextMeshProUGUI characterNameText = null;
    [SerializeField] private Character_DB CharacterDB = null;
    [SerializeField] private Transform charactersHolder;
    [SerializeField] private CharacterSelectButton characterSelectButtonPrefab;
    [SerializeField] private PlayerCard[] playerCards;

    private void Awake()
    {
        Players = new NetworkList<CharacterSelectState>();
    }
    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            Player_characters_SO[] characters = CharacterDB.GetAllCharacters();
            foreach (Player_characters_SO character in characters)
            {
                var button = Instantiate(characterSelectButtonPrefab, charactersHolder);
                button.SetCharacter(character, this);
            }
            Players.OnListChanged += HandlePlayersStateChanged;
        }
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientDisConnected;
        }
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            HandleClientConnected(client.ClientId);
            Debug.Log("Client Connected: " + client.ClientId);
        }
    }


    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            Players.OnListChanged -= HandlePlayersStateChanged;

        }
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientDisConnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        Players.Add(new CharacterSelectState(clientId));
    }

    private void HandleClientDisConnected(ulong clientId)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].ClientId == clientId)
            {
                Players.RemoveAt(i);
                return;
            }
        }
    }

    public void Select(Player_characters_SO character)
    {
        characterNameText.text = character.CharacterName;

        characterInfoPanel.SetActive(true);
        SelectServerRPC(character.Id);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRPC(int characterId, ServerRpcParams serverrpcParams = default)
    {
        for(int i = 0; i < Players.Count; i++)
        {
            if(Players[i].ClientId == serverrpcParams.Receive.SenderClientId)
            {
                Players[i] = new CharacterSelectState(Players[i].ClientId, characterId);
                return;
            }
        }
    }

    private void HandlePlayersStateChanged(NetworkListEvent<CharacterSelectState> changeEvent)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if(Players.Count >i)
            {
                playerCards[i].UpdateDisplay(Players[i]);
            }
            else
            {
                playerCards[i].HideDisplay();
            }
        }


    }
}
