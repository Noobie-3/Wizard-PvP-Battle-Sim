using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    public PlayerSpawnLocation[] spawnPoints;
    private Dictionary<ulong, NetworkObject> playerPrefabs = new Dictionary<ulong, NetworkObject>();
    private Dictionary<ulong, NetworkObject> wandPrefabs = new Dictionary<ulong, NetworkObject>(); // Store wand prefabs by WandID
    public  Dictionary<ulong, PlayerSpawnLocation> playerSpawnPoints = new Dictionary<ulong, PlayerSpawnLocation>();
   public  WandDatabase WD;
    public  CharacterDatabase CD;
    public NetworkObject PlayerNotifier;
    public static SpawnManager instance;

    private bool gameStarted = false;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

    }

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        NetworkManager.Singleton.SceneManager.OnSceneEvent += AssignSpawnPointsByServer;
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



    public void AssignSpawnPointsByServer(SceneEvent sceneevent)
    {
        if (sceneevent.SceneEventType == SceneEventType.LoadEventCompleted && sceneevent.SceneName != gameController.GC.CharacterSelectSceneName && sceneevent.SceneName != gameController.GC.EndScreenSceneName)
        {
            spawnPoints = GameObject.FindObjectsOfType<PlayerSpawnLocation>();
            print(spawnPoints.Length + "  this is how many spawn points there is");

            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                // Get or assign a spawn point
                var assignedSpawnPoint = GetAvailableSpawnPoint();
                playerSpawnPoints[client.Key] = assignedSpawnPoint;
                print("Client with key of " + client.Key + "has its spawn point set at " + playerSpawnPoints[client.Key].transform.position );
                if(IsHost)
                {
                    SpawnPlayerServerRpc(client.Key);
                }

            }
        }

    }



    // Spawns player with their selected wand
    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId)
    {
        



        var PlayerState = PlayerStateManager.Singleton.LookupState(clientId);
        // Instantiate and spawn the player
        print(PlayerState.CharacterId + " Character ID");

        GameObject playerInstance = Instantiate(CD.GetCharacterById(PlayerState.CharacterId).GameplayPrefab, playerSpawnPoints[clientId].transform.position, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        //playerInstance.GetComponent<SpellCaster>().SetSpell(clientId);
        Debug.Log($"Spawned player {clientId} at {playerSpawnPoints[clientId].transform.position}");
        Transform handTransform = playerInstance.GetComponent<SpellCaster>().Hand;
        GameObject wandInstance = Instantiate(WD.GetWandById(PlayerState.WandID).WandPrefab, handTransform.position, handTransform.rotation);
        wandInstance.transform.SetParent(handTransform); // Attach wand to player's hand

        Debug.Log($"Spawned wand {WD.GetWandById(PlayerState.WandID).DisplayName} for player {clientId} at hand position {handTransform.position}");

        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject = playerInstance.GetComponent<NetworkObject>();
        print("Spawned with owndership of id  " + clientId);


        //Untested change.. changed network object to gameobject    if it doenst funtion revert back
    }

    [ServerRpc (RequireOwnership = false)]
    public void RespawnPlayerServerRpc(ulong clientId)
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
            Destroy(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject);
        }
        else
        {
            print("Could not find the Player Object for client id " + clientId);
        }

        SpawnPlayerServerRpc(clientId);

    }
    // Helper method to get an available spawn point
    private PlayerSpawnLocation GetAvailableSpawnPoint()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.IsAvailable.Value == false)
            {
                spawnPoint.SetAvailability(true);
                return spawnPoint;
            }
            else {
                continue; }
        }

        print("Resverting to default spawn point");
        if(spawnPoints.Length == 0)
        {
            return null;
        }
        return spawnPoints[0];

    }

    public void ResetSpawnPoints()
    {
        if (spawnPoints.Length != 0)
        {
            foreach (var spawnPoint in spawnPoints)
            {
                spawnPoint.SetAvailability(false);
            }
        }
        }

}
