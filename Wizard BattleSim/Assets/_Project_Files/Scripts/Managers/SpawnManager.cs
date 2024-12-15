using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    public PlayerSpawnLocation[] spawnPoints;
    private Dictionary<ulong, NetworkObject> playerPrefabs = new Dictionary<ulong, NetworkObject>();
    private Dictionary<ulong, NetworkObject> wandPrefabs = new Dictionary<ulong, NetworkObject>(); // Store wand prefabs by WandID
    private Dictionary<ulong, PlayerSpawnLocation> playerSpawnPoints = new Dictionary<ulong, PlayerSpawnLocation>();
   public  WandDatabase WD;
    public  CharacterDatabase CD;

    public static SpawnManager instance;

    private bool gameStarted = false;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }



    // Called to start the game and load the scene
    [ServerRpc]
    public void StartGameServerRpc()
    {
        if (!IsServer || gameStarted) return; // Ensure this runs only once on the server
        gameStarted = true;

        Debug.Log("Starting game and loading scene.");
        NetworkManager.Singleton.SceneManager.LoadScene("Scene_01", UnityEngine.SceneManagement.LoadSceneMode.Single);

    }

    // Callback when the scene loads, only runs on the server
    private void SpawnPlayersOnSceneLoad(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadMode)
    {
        if (sceneName == "Scene_01" && IsServer)
        {
            Debug.Log("Scene loaded on the server, spawning players.");

            // Spawn each registered player
            foreach (var playerEntry in NetworkManager.ConnectedClients)
            { 
                Debug.Log($"Spawning player {playerEntry}");
                SpawnPlayer(playerEntry.Key);
            }

        }
    }

    // Spawns player with their selected wand
    public void SpawnPlayer(ulong clientId)
    {
        // Check if player is already spawned
        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
        {
            Debug.Log($"Player {clientId} is already spawned. Skipping duplicate spawn.");
            return;
        }

        // Get or assign a spawn point

            var assignedSpawnPoint = GetAvailableSpawnPoint();
            if (assignedSpawnPoint == null)
            {
                Debug.LogError("No available spawn points!");
                return;
            }

            playerSpawnPoints[clientId] = assignedSpawnPoint;
            assignedSpawnPoint.SetAvailability(false);
        

        var PlayerState = PlayerStateManager.Singleton.LookupState(clientId);
        // Instantiate and spawn the player
        print(PlayerState.CharacterId + " Character ID");
        NetworkObject playerInstance = Instantiate(CD.GetCharacterById(PlayerState.CharacterId).GameplayPrefab, assignedSpawnPoint.transform.position, Quaternion.identity);
        playerInstance.GetComponent<SpellCaster>().SetSpell(clientId);
        playerInstance.SpawnWithOwnership(clientId);

        Debug.Log($"Spawned player {clientId} at {assignedSpawnPoint.transform.position}");


            Transform handTransform = playerInstance.GetComponent<SpellCaster>().Hand;
            GameObject wandInstance = Instantiate(WD.GetWandById(PlayerState.WandID).WandPrefab, handTransform.position, handTransform.rotation);
            wandInstance.transform.SetParent(handTransform); // Attach wand to player's hand
            
            Debug.Log($"Spawned wand {WD.GetWandById(PlayerState.WandID).DisplayName} for player {clientId} at hand position {handTransform.position}");
        
    }

    public void RespawnPlayer(ulong clientId)
    {
        // Check if the client has an assigned spawn point
        if (!playerSpawnPoints.TryGetValue(clientId, out PlayerSpawnLocation spawnPoint) || spawnPoint == null)
        {
            Debug.LogError($"No spawn point assigned for client {clientId}. Cannot respawn.");
            return;
        }

        // Check if the player's NetworkObject is valid and remove the old instance if needed
        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
        {
            print("Destoryed the Player Object");
            Destroy(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject);
        }
        else
        {
            print("Could not find the Player Object for client id " + clientId);
        }


        // Instantiate and spawn a new player instance at the assigned spawn point
        var PlayerState = PlayerStateManager.Singleton.LookupState(clientId);
        NetworkObject playerInstance = Instantiate(CD.characters[PlayerState.CharacterId].GameplayPrefab, spawnPoint.transform.position,
            Quaternion.identity); playerInstance.SpawnWithOwnership(clientId);
        Debug.Log($"Respawned player {clientId} at {spawnPoint.transform.position} players actual position is {playerInstance.transform.position}");

        Transform handTransform = playerInstance.GetComponent<SpellCaster>().Hand;
        GameObject wandInstance = Instantiate(WD.Wands[PlayerState.WandID].WandPrefab, handTransform.position, handTransform.rotation);
        wandInstance.transform.SetParent(handTransform); // Attach wand to player's hand
        Debug.Log($"Spawned wand {WD.Wands[clientId].DisplayName} for player {clientId} at hand position {handTransform.position}");
    }
    // Helper method to get an available spawn point
    private PlayerSpawnLocation GetAvailableSpawnPoint()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.IsAvailable)
            {
                return spawnPoint;
            }
        }
        return null; // Return null if no spawn point is available
    }


}
