using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class ServerManager : NetworkBehaviour
{
    public static ServerManager Instance { get; private set; }
    [SerializeField] CharacterSelectDisplay characterSelectDisplay;
    public WandSelectDisplay wandSelectDisplay;
    [SerializeField] private CharacterDatabase characterDatabase;
    public Dictionary<ulong, int> playerCharacterIds = new Dictionary<ulong, int>();
    public Coroutine DataPrintCoRo;
    public WandDatabase WandDataBase;
    public GameObject CommandConsole;
    public Lobby Lobby;
    public Map_DB MapDB;
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
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

    }
    // This method will be called every time a client connects
    private void OnClientConnected(ulong clientId)
    {
            SpawnLocalObject();
        
    }

    // Spawn object locally for the player
    private void SpawnLocalObject()
    {
        // Instantiate the prefab locally for the player
        GameObject spawnedObject = Instantiate(CommandConsole);

        spawnedObject.transform.position = new Vector3(0, 0, 0);  // Example position

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




    public void StartGame()
    {
        print("Called Start Game in Server Managaer");
        StopCoroutine(DataPrintCoRo);

        if (IsHost)
        {
            StartGameServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc()
    {
        print(playerCharacterIds.Count + " PLayer character ids");

        // Load the new scene for all clients and the server

            print(Lobby.Data["Level"].Value + "this is what the lobby data sayas the map is");
            int convertedString = int.Parse(Lobby.Data["Level"].Value);
            print("Converted string: " + convertedString + " and the map name is " + MapDB.GetMapById(convertedString).MapName);
            NetworkManager.Singleton.SceneManager.LoadScene(MapDB.GetMapById(convertedString).MapName, LoadSceneMode.Single);
            NetworkManager.Singleton.SceneManager.OnLoadComplete += StartGameHelper;
        


    }
    public void StartGameHelper(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadMode)
    {   // Register a callback to spawn players after the scene loads
        print("Called Start Game helper in Server MNager");
        // Check if the server loaded the scene
        SpawnManager.instance.spawnPoints = FindObjectsOfType<PlayerSpawnLocation>();
        // Spawn players for each client after the scene loads

        foreach (var playerEntry in PlayerStateManager.Singleton.AllStatePlayers)
        {
            print("made it to the foreach loop for player entries");

            print("Called Spawn Player  in Server Manager{Depercrated}");
/*            SpawnManager.instance.SpawnPlayer(playerEntry.ClientId);
*/        }
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= StartGameHelper;
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
        }

    }

    //check if most  users are locked in  if not start a timer on their end to lock them in 
    public void CheckLockIn()
    {
        int lockedIn = 0;
        foreach (var player in characterSelectDisplay.players)
        {
            if (player.IsLockedIn)
            {
                lockedIn++;
            }
        }

        //if 70 percent of the players are locked in start the game after a time and start a timer on the ones that are not locked in screen
        if(lockedIn * 100 / characterSelectDisplay.players.Count >= 70)
        {
            StartGame();
        }
        //start timer to lock in before starting game
        else
        {

        }
    }

    public void replayGame()
    {

    }

    public async void LeaveLobby()
    {
        try
        {
            if (Lobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(Lobby.Id, AuthenticationService.Instance.PlayerId);
                Lobby = null;
                print("Left lobby");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

}