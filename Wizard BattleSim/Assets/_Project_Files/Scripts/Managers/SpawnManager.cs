using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    public PlayerSpawnLocation[] spawnPoints;
    private Dictionary<ulong, NetworkObject> playerPrefabs = new Dictionary<ulong, NetworkObject>();
    private Dictionary<ulong, PlayerSpawnLocation> playerSpawnPoints = new Dictionary<ulong, PlayerSpawnLocation>(); // To track assigned spawn points
    public static SpawnManager instance;

    private bool gameStarted = false; // Prevents multiple starts

    private void Awake()
    {
        instance = this;
    }

    // Method to register each player's prefab
    public void RegisterPlayerPrefab(ulong clientId, NetworkObject prefab)
    {
        if (!playerPrefabs.ContainsKey(clientId))
        {
            playerPrefabs[clientId] = prefab;
        }
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
            foreach (var playerEntry in playerPrefabs)
            {
                ulong playerId = playerEntry.Key;
                NetworkObject playerPrefab = playerEntry.Value;

                Debug.Log($"Spawning player {playerId}");
                SpawnPlayer(playerId, playerPrefab);
            }

        }
    }

    // Method to spawn a player at their assigned or available spawn point
    public void SpawnPlayer(ulong clientId, NetworkObject playerPrefab)
    {
        // Check if player is already spawned
        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
        {
            Debug.Log($"Player {clientId} is already spawned. Skipping duplicate spawn.");
            return;
        }

        // Check if the player already has an assigned spawn point
        if (!playerSpawnPoints.TryGetValue(clientId, out PlayerSpawnLocation assignedSpawnPoint))
        {
            // Assign a new spawn point if one is not already assigned
            assignedSpawnPoint = GetAvailableSpawnPoint();
            if (assignedSpawnPoint == null)
            {
                Debug.LogError("No available spawn points!");
                return;
            }

            playerSpawnPoints[clientId] = assignedSpawnPoint;
            assignedSpawnPoint.SetAvailability(false); // Mark spawn point as unavailable
        }

        // Instantiate and spawn the player at the assigned spawn point
        NetworkObject playerInstance = Instantiate(playerPrefab, assignedSpawnPoint.transform.position, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
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
            print("Could not find the Player Object for client id "+ clientId);
        }

        // Instantiate and spawn a new player instance at the assigned spawn point
        if (playerPrefabs.TryGetValue(clientId, out NetworkObject playerPrefab))
        {
            NetworkObject playerInstance = Instantiate(playerPrefab, spawnPoint.transform.position, Quaternion.identity);
            playerInstance.SpawnWithOwnership(clientId);

            Debug.Log($"Respawned player {clientId} at {spawnPoint.transform.position}");
        }
        else
        {
            Debug.LogError($"No prefab registered for client {clientId}. Cannot respawn.");
        }
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
