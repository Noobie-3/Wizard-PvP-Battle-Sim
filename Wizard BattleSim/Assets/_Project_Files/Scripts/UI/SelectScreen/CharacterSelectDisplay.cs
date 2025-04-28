using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] public Transform charactersHolder;
    [SerializeField] private CharacterSelectButton selectButtonPrefab;
    [SerializeField] public PlayerCard[] playerCards;
    [SerializeField] private GameObject characterInfoPanel;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Transform introSpawnPoint;
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private Button lockInButton;
    [SerializeField] private Button StartButton;
    [SerializeField] ServerManager serverManager;
    [SerializeField] private  float  Timer = 0;

    private GameObject introInstance;
    private List<CharacterSelectButton> characterButtons = new List<CharacterSelectButton>();
    public NetworkList<CharacterSelectState> players;


    private void Awake()
    {
        players = new NetworkList<CharacterSelectState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
/*            StartButton.gameObject.SetActive(false);
*/            Character[] allCharacters = characterDatabase.GetAllCharacters();

            foreach (var character in allCharacters)
            {
                var selectbuttonInstance = Instantiate(selectButtonPrefab, charactersHolder);
                selectbuttonInstance.SetCharacter(this, character);
                characterButtons.Add(selectbuttonInstance);
            }

            players.OnListChanged += HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            // Handle already connected clients (including the host)
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }
        if(IsHost)
        {
            //StartButton.gameObject.SetActive(true);
        }
    }



    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        // Check if the clientId already exists in the players list to prevent duplicate entries
        foreach (var player in players)
        {
            if (player.ClientId == clientId)
            {
                Debug.Log($"Client {clientId} already exists. Skipping duplicate add.");
                return;
            }
        }

        // Add the new player to the list
        players.Add(new CharacterSelectState(clientId));
        Debug.Log($"Client {clientId} added. Total players: {players.Count}");
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == clientId)
            {
                players.RemoveAt(i);
                Debug.Log($"Client {clientId} removed from the player list. Total players: {players.Count}");
                break;
            }
        }
    }

    public void Select(Character character)
    {
        for (int i = 0; i < players.Count; i++)
        {
            // Skip if the player is not the local player
            if (players[i].ClientId != NetworkManager.Singleton.LocalClientId) { continue; }
            // Skip if the player is already locked in
            if (players[i].IsLockedIn) { return; }
            // Skip if the player is selecting the same character
            if (players[i].CharacterId == character.Id) { return; }
            // Skip if the character is already taken
            if (IsCharacterTaken(character.Id, false)) { return; }
        }

        characterNameText.text = character.DisplayName;

        characterInfoPanel.SetActive(true);

        if (introInstance != null)
        {
            Destroy(introInstance);
        }

        introInstance = Instantiate(character.IntroPrefab, introSpawnPoint);

        SelectServerRpc(character.Id);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRpc(int characterId, ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != serverRpcParams.Receive.SenderClientId) { continue; }

            if (!characterDatabase.IsValidCharacterId(characterId)) { return; }

            if (IsCharacterTaken(characterId, true)) { return; }
            print("old values of player state" + players[i].ClientId + " " + players[i].CharacterId + " " + players[i].WandID + " " + players[i].Spell0 + " " + players[i].Spell1 + " " + players[i].Spell2 + " " + players[i].IsLockedIn);
            players[i] = new CharacterSelectState(
                players[i].ClientId,
                characterId, 0,0,1,2,
                players[i].IsLockedIn,
                players[i].PlayerLobbyId
            );
            print("new values of player state" + players[i].ClientId + " " + players[i].CharacterId + " " + players[i].WandID + " " + players[i].Spell0 + " " + players[i].Spell1 + " " + players[i].Spell2 + " " + players[i].IsLockedIn);

        }
    }

    public void LockIn()
    {
        LockInServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void LockInServerRpc(ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != serverRpcParams.Receive.SenderClientId) { continue; }

            if (!characterDatabase.IsValidCharacterId(players[i].CharacterId)) { return; }

            if (IsCharacterTaken(players[i].CharacterId, true)) { return; }
            players[i] = new CharacterSelectState(
                players[i].ClientId,
                players[i].CharacterId,
                players[i].WandID,0,1,2,
                true
                , players[i].PlayerLobbyId
            );
        }

        foreach (var player in players)
        {
            if (!player.IsLockedIn) { return; }
        }
    }


    private void UpdateDisplayTimed()
    {
        if(Timer > 0)
        {
            Timer -= Time.deltaTime;
        }
        else
        {
            Timer = 1.5f;
        }
    }

    private void FixedUpdate()
    {
        if (IsClient)
        {
            UpdateDisplayTimed();
        }
    }

    private void HandlePlayersStateChanged(NetworkListEvent<CharacterSelectState> changeEvent)
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            if (players.Count > i)
            {
                playerCards[i].UpdateDisplay(players[i]);
            }
            else
            {
                playerCards[i].DisableDisplay();
            }
        }

/*        foreach (var button in characterButtons)
        {
            if (button.IsDisabled) { continue; }

            if (IsCharacterTaken(button.Character.Id, false))
            {
                button.SetDisabled();
            }
        }

*//*        foreach (var player in players)
        {
            if (player.ClientId != NetworkManager.Singleton.LocalClientId) { continue; }

            if (player.IsLockedIn)
            {
                lockInButton.interactable = false;
                break;
            }

            if (IsCharacterTaken(player.CharacterId, false))
            {
                lockInButton.interactable = false;
                break;
            }

            lockInButton.interactable = true;

            break;
        }
*/

    }

    private bool IsCharacterTaken(int characterId, bool checkAll)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (!checkAll)
            {
                if (players[i].ClientId == NetworkManager.Singleton.LocalClientId) { continue; }
            }

            if (players[i].IsLockedIn && players[i].CharacterId == characterId)
            {
                return true;
            }
        }

        return false;
    }


    //Start Game Button
    public void StartGame()
    {if (!IsServer) return;
        if(players.Count < 2)
        {
            print("Not enough players to start game");
            return;
        }

        print("Made the Player selectrions");
        print(players.Count + " Player count state");
        foreach(var player in players)
        {
            print("added player state" + player.ClientId);
            PlayerStateManager.Singleton.AddState(player);
        }
        serverManager.StartGame();
    }



}
