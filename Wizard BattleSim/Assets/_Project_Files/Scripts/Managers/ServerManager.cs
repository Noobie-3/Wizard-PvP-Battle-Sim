using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class ServerManager : NetworkBehaviour
{
    public static ServerManager Instance { get; private set; }
    [SerializeField] CharacterSelectDisplay characterSelectDisplay;
    [SerializeField] private CharacterDatabase characterDatabase;
    public Dictionary<ulong, int> playerCharacterIds = new Dictionary<ulong, int>();
    public Coroutine DataPrintCoRo;

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

        DataPrintCoRo = StartCoroutine(PrintData());

    }

    private void FixedUpdate()
    {
        if (characterSelectDisplay != null)
        {

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


        }
    }



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
        StopCoroutine(DataPrintCoRo);

        if (IsHost)
        {
            StartGameServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
private void StartGameServerRpc()
    {
        print(playerCharacterIds.Count);

        // Load the new scene for all clients and the server
        NetworkManager.Singleton.SceneManager.LoadScene("Scene_01", LoadSceneMode.Single);
        NetworkManager.Singleton.SceneManager.OnLoadComplete += StartGameHelper;


    }
    public void StartGameHelper(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadMode)
    {   // Register a callback to spawn players after the scene loads
        
        // Check if the server loaded the scene
        SpawnManager.instance.spawnPoints = FindObjectsOfType<PlayerSpawnLocation>();
        // Spawn players for each client after the scene loads
        foreach (var playerEntry in playerCharacterIds)
        {
            SpawnPlayer(playerEntry.Key, characterDatabase.GetCharacterById(playerEntry.Value)); // Pass in clientId and prefab
        }
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= StartGameHelper;
    }

    private void SpawnPlayer(ulong clientId, Character Player)
    {

        
        // Register and spawn the player on the server
        SpawnManager.instance.RegisterPlayerPrefab(clientId, Player.GameplayPrefab);
        SpawnManager.instance.SpawnPlayer(clientId, Player.GameplayPrefab);
    }

    public IEnumerator PrintData()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            

            // Print out dictionary
            foreach (var item in playerCharacterIds)
            {
                Debug.Log("Key: " + item.Key + " Value: " + item.Value);
            }
            print(characterSelectDisplay.players.Count + " Player Count in Select Display");
        }

    }
}