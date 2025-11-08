using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WebSocketSharp;

public class PlayerStateManager : NetworkBehaviour
{
    [SerializeField] public NetworkList<CharacterSelectState> AllStatePlayers = new NetworkList<CharacterSelectState>();

    public static PlayerStateManager Singleton;

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Singleton != this)
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.OnClientConnectedCallback += OnPlayerSpawn;
        NetworkManager.OnClientDisconnectCallback += OnPlayerDespawn;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            OnPlayerSpawn(client.ClientId);
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnPlayerSpawn;
            NetworkManager.OnClientDisconnectCallback -= OnPlayerDespawn;
        }
    }

    public void OnPlayerSpawn(ulong clientId)
    {

        var state = new CharacterSelectState(
            clientId: clientId,
            characterId: -1,
            wandID: -1,
            spell0: 0,
            spell1: 1,
            spell2: 2,
            isLockedIn: false,
            playerLobbyId: "",
            winCount: 0,
            ranking: 0
            
        );

        RequestAddOrUpdateStateServerRpc(state);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestAddOrUpdateStateServerRpc(CharacterSelectState newState)
    {
        Debug.Log($"[ServerRpc] State update requested by ClientId: {newState.ClientId}");
        AddState(newState); // Safely run on server
    }


    public void OnPlayerDespawn(ulong clientId)
    {
        if (!IsServer) return;

        var state = LookupState(clientId);
        if (state.ClientId != 0)
        {
            AllStatePlayers.Remove(state);
            Debug.Log($"[Server] Player {clientId} removed from state list.");
        }
    }

    public void AddState(CharacterSelectState newState)
    {
        if (!IsServer) return;

        var found = false;

        for (int i = 0; i < AllStatePlayers.Count; i++)
        {
            if (AllStatePlayers[i].ClientId == newState.ClientId)
            {
                var existing = AllStatePlayers[i];

                if (newState.CharacterId != -1)
                    existing.CharacterId = newState.CharacterId;

                if (newState.WandID != -1)
                    existing.WandID = newState.WandID;

                if (newState.Spell0 != -1)
                    existing.Spell0 = newState.Spell0;

                if (newState.Spell1 != -1)
                    existing.Spell1 = newState.Spell1;

                if (newState.Spell2 != -1)
                    existing.Spell2 = newState.Spell2;

                if (!string.IsNullOrEmpty(newState.PlayerLobbyId.ToString()))
                    existing.PlayerLobbyId = newState.PlayerLobbyId;

                existing.IsLockedIn = newState.IsLockedIn;

                existing.WinCount = newState.WinCount;
                existing.Ranking = newState.Ranking;
                if (!string.IsNullOrEmpty(newState.PLayerDisplayName.ToString()))
                    existing.PLayerDisplayName = newState.PLayerDisplayName;

                AllStatePlayers[i] = existing;
                found = true;

                Debug.Log($"[Server] Player {existing.ClientId} spawned and state added. Values are  Client Id: {newState.ClientId} Character Id: {newState.CharacterId} Wand Id {newState.WandID} Spells 1:{newState.Spell0} 2: {newState.Spell1} 3:{newState.Spell2}  isLockedIn: {newState.IsLockedIn} playerLobbyId: {newState.PlayerLobbyId} playerdisplayName: {newState.PLayerDisplayName} Playerwins: {newState.WinCount}");

                break;
            }
        }

        if (!found)
        {
            AllStatePlayers.Add(newState);
            Debug.Log($"[Server] Player {newState.ClientId} spawned and state added. Values are  Client Id: {newState.ClientId} Character Id: {newState.CharacterId} Wand Id {newState.WandID} Spells 1:{newState.Spell0} 2: {newState.Spell1} 3:{newState.Spell2}  isLockedIn: {newState.IsLockedIn} playerLobbyId: {newState.PlayerLobbyId} playerdisplayName: {newState.PLayerDisplayName} Player wins: {newState.WinCount}");
        }
    }

    public void ResetAllWinsServer()
    {
        if(IsClient && !IsServer)
        {
            print("Client Made it to reset for some reason fix this now");
            return;
        }
        for (int i = 0; i < AllStatePlayers.Count; i++)
        {

            var s = AllStatePlayers[i];

            s.WinCount = 0;
            s.Ranking = 0;
            AllStatePlayers[i] = s; // assign back to trigger replication
            if (IsServer)
            {
                print("Server Reset " + s.PLayerDisplayName + " player id is " + s.ClientId + " their new win count is " + AllStatePlayers[i].WinCount);
            }
        }
    }


    public CharacterSelectState LookupState(ulong clientId)
    {
        foreach (var state in AllStatePlayers)
        {
            if (state.ClientId == clientId)
                return state;
        }

        Debug.LogWarning($"[Server] Could not find state for ClientId {clientId}");
        return default;
    }
}
