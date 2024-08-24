using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ServerManager : NetworkBehaviour
{
    public static ServerManager Instance { get; private set; }
    [SerializeField] CharacterSelectDisplay characterSelectDisplay;
    [SerializeField] private CharacterDatabase characterDatabase;
    public Dictionary<ulong, int> playerCharacterIds = new Dictionary<ulong, int>();
    // Start is called before the first frame update
    void Start()
    {

        DontDestroyOnLoad(this.gameObject);

        //Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }



    }

    private void FixedUpdate()
    {
        if (characterSelectDisplay != null)
        {
            print(characterSelectDisplay.players.Count + " Player Count in Select Display");

            // Update dictionary with current players
            foreach (var player in characterSelectDisplay.players)
            {
                // If entry does not exist in dictionary, add it
                if (!playerCharacterIds.ContainsKey(player.ClientId))
                {
                    playerCharacterIds.Add(player.ClientId, player.CharacterId);
                }
                // If entry exists in dictionary, update it
                else
                {
                    playerCharacterIds[player.ClientId] = player.CharacterId;
                }
            }

            // Collect keys to remove
            List<ulong> keysToRemove = new List<ulong>();

            foreach (var clientId in playerCharacterIds.Keys)
            {
                bool found = false;
                foreach (var player in characterSelectDisplay.players)
                {
                    if (player.ClientId == clientId)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    keysToRemove.Add(clientId);
                }
            }

            // Remove keys outside of the loop
            foreach (var key in keysToRemove)
            {
                playerCharacterIds.Remove(key);
            }

            // Print out dictionary
            foreach (var item in playerCharacterIds)
            {
                Debug.Log("Key: " + item.Key + " Value: " + item.Value);
            }
        }
    }


    private void SpawnPlayer(int CharacterID, ulong ClientId)
    {
        //Spawn player
        if (IsClient)
        {

            SpawnPlayerServerRpc(CharacterID, ClientId);
        }

    }
    //Server RPc to spawn player

    [ServerRpc]
    private void SpawnPlayerServerRpc(int characterID, ulong clientId)
    {
        //Spawn player for a client
        // Retrieve the player prefab from the character database
        var playerPrefabID = characterDatabase.GetCharacterById(characterID);
        NetworkObject playerPrefab = playerPrefabID.GameplayPrefab;

        // Instantiate the player prefab
        var playerInstance = Instantiate(playerPrefab);

        // Spawn the player object on the network and assign ownership to the client
        playerInstance.SpawnAsPlayerObject(clientId);

        // Ensure the player object is not destroyed when the scene changes
        DontDestroyOnLoad(playerInstance.gameObject);

        // Log the name of the spawned player object
        print(playerInstance.gameObject.name + " Spawned");
    }

    public void StartGame()
    {
        if (IsHost)
        {
            StartGameServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc()
    {
        print(playerCharacterIds.Count);
        // Spawn players for each client after the scene is 
        foreach(var Player in playerCharacterIds)
        {
            SpawnPlayer(Player.Value, Player.Key);
        }
        // Load the new scene on the server and all clients
        NetworkManager.Singleton.SceneManager.LoadScene("Scene_01", UnityEngine.SceneManagement.LoadSceneMode.Single);

    }
}