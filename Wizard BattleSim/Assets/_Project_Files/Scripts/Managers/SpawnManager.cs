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
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnNotifier;
    }

    public void SpawnNotifier(ulong Clientid)
    {
        var Notifier = Instantiate(PlayerNotifier);
        Notifier.GetComponent<PlayerSceneNotifier>().CLientId = Clientid;
        Notifier.SpawnWithOwnership(Clientid);
        
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



    public void AssignSpawnPointsByServer()
    {

        foreach(var client in NetworkManager.Singleton.ConnectedClients)
        {
            // Get or assign a spawn point
            spawnPoints = FindObjectsByType<PlayerSpawnLocation>(sortMode: FindObjectsSortMode.None);
            print(spawnPoints.Length + "  this is how many spawn points there is");
            var assignedSpawnPoint = GetAvailableSpawnPoint();
            playerSpawnPoints[client.Key] = assignedSpawnPoint;

        }


    }



    // Spawns player with their selected wand
    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId)
    {

        // Check if player is already spawned
        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
        {
            Debug.Log($"Player {clientId} is already spawned. Skipping duplicate spawn.");
            return;
        }


        var PlayerState = PlayerStateManager.Singleton.LookupState(clientId);
        // Instantiate and spawn the player
        print(PlayerState.CharacterId + " Character ID");

        NetworkObject playerInstance = Instantiate(CD.GetCharacterById(PlayerState.CharacterId).GameplayPrefab, playerSpawnPoints[clientId].transform.position, Quaternion.identity);
        //playerInstance.GetComponent<SpellCaster>().SetSpell(clientId);
        Debug.Log($"Spawned player {clientId} at {playerSpawnPoints[clientId].transform.position}");
        Transform handTransform = playerInstance.GetComponent<SpellCaster>().Hand;
        GameObject wandInstance = Instantiate(WD.GetWandById(PlayerState.WandID).WandPrefab, handTransform.position, handTransform.rotation);
        wandInstance.transform.SetParent(handTransform); // Attach wand to player's hand

        Debug.Log($"Spawned wand {WD.GetWandById(PlayerState.WandID).DisplayName} for player {clientId} at hand position {handTransform.position}");

        playerInstance.SpawnWithOwnership(clientId);
        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject = playerInstance;
        print("Spawned with owndership of id  " + clientId);

    }

    public void RespawnPlayer(ulong clientId)
    {
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
        return spawnPoints[0];

    }


}
