using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinTracker : NetworkBehaviour
{
    public int winsNeeded = 3;
    public static WinTracker Singleton;

    public GameObject WinImage;
    public GameObject LoseImage;
    public Image[] PlayerImages;
    public CharacterDatabase characterDatabase;

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddWin(ulong clientId)
    {
        var currentState = PlayerStateManager.Singleton.LookupState(clientId);
        var updatedState = new CharacterSelectState(
            clientId: currentState.ClientId,
            characterId: currentState.CharacterId,
            wandID: currentState.WandID,
            spell0: currentState.Spell0,
            spell1: currentState.Spell1,
            spell2: currentState.Spell2,
            isLockedIn: currentState.IsLockedIn,
            playerLobbyId: currentState.PlayerLobbyId,
            PlayerName: currentState.PLayerDisplayName,
            winCount: currentState.WinCount + 1
        );

        PlayerStateManager.Singleton.AddState(updatedState);
        print("Added Win to Player: " + updatedState.PLayerDisplayName.ToString() + " | New Win Count: " + updatedState.WinCount);
        if (updatedState.WinCount >= winsNeeded)
        {
            EndGame(clientId);
            print("Player: " + updatedState.PLayerDisplayName.ToString() + " has won the game!");
        }
    }

    private void EndGame(ulong winningClientId)
    {
        if (!IsServer) return;

        // Destroy player objects
        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (player.PlayerObject != null)
            {
                Destroy(player.PlayerObject.gameObject);
                print("Destroyed Player Object for: " + PlayerStateManager.Singleton.LookupState(player.ClientId).PLayerDisplayName);
            }
        }

        // Load win screen scene
        if (!string.IsNullOrEmpty(gameController.GC.EndScreenSceneName))
        {
            print("Loading End Screen Scene: " + gameController.GC.EndScreenSceneName);
            NetworkManager.Singleton.SceneManager.LoadScene(gameController.GC.EndScreenSceneName, LoadSceneMode.Single);
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnWinSceneLoaded;
        }
        else
        {
            Debug.LogError("End screen scene name is not set in GameController.");
        }
    }

    private void OnWinSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == gameController.GC.EndScreenSceneName && IsServer)
        {
            SendFinalResultsToClients();
        }

    }

    private void SendFinalResultsToClients()
    {
        List<CharacterSelectState> copiedStates = new List<CharacterSelectState>();
        foreach (var state in PlayerStateManager.Singleton.AllStatePlayers)
        {
            copiedStates.Add(state);
        }

        var sorted = copiedStates
            .OrderByDescending(p => p.WinCount)
            .ToList();

        CharacterSelectState[] rankedStates = new CharacterSelectState[sorted.Count];

        for (int i = 0; i < sorted.Count; i++)
        {
            rankedStates[i] = new CharacterSelectState(
                clientId: sorted[i].ClientId,
                characterId: sorted[i].CharacterId,
                wandID: sorted[i].WandID,
                spell0: sorted[i].Spell0,
                spell1: sorted[i].Spell1,
                spell2: sorted[i].Spell2,
                isLockedIn: sorted[i].IsLockedIn,
                playerLobbyId: sorted[i].PlayerLobbyId,
                PlayerName: sorted[i].PLayerDisplayName,
                winCount: sorted[i].WinCount,
                ranking: i + 1
            );
        }


        foreach(var state in rankedStates)
        {
            print("Player Name: " + state.PLayerDisplayName.ToString() + " Player Rank: " + state.Ranking);
        }


        DisplayWinResultsClientRpc(rankedStates);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetWinsServerRpc()
    {
        print("Resetting all player wins.");
        foreach (var state in PlayerStateManager.Singleton.AllStatePlayers)
        {
            var resetState = new CharacterSelectState(
                clientId: state.ClientId,
                characterId: state.CharacterId,
                wandID: state.WandID,
                spell0: state.Spell0,
                spell1: state.Spell1,
                spell2: state.Spell2,
                isLockedIn: state.IsLockedIn,
                playerLobbyId: state.PlayerLobbyId,
                PlayerName: state.PLayerDisplayName,
                winCount: 0,
                ranking: 0
            );

            PlayerStateManager.Singleton.AddState(resetState);
            print("Reset Win Count for Player: " + resetState.PLayerDisplayName.ToString());
        }
    }

    [ClientRpc]
    private void DisplayWinResultsClientRpc(CharacterSelectState[] sortedResults)
    {
        print("Displaying Win Results on Client");
        print("PLayer Name: " + sortedResults[0].PLayerDisplayName.ToString() + " Player Rank: " + sortedResults[0].Ranking);
        if (WinScreen_Load.Instance != null)
        {
            WinScreen_Load.Instance.ShowResults(sortedResults);
        }
        
    }
}
