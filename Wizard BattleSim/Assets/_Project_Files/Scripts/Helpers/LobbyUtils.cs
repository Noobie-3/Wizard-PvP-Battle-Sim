using Unity.Services.Lobbies.Models;
using UnityEngine;

public static class LobbyUtils
{
    public static string GetPlayerNameFromLobby(string lobbyId)
    {
        var lobby = ServerManager.Instance.Lobby;
        if (lobby == null)
        {
            Debug.LogWarning("Lobby is null in LobbyUtils.");
            return "Unknown (No Lobby)";
        }

        foreach (var player in lobby.Players)
        {
            Debug.Log($"Lobby player ID: {player.Id}");

            if (player.Id == lobbyId)
            {
                if (player.Data != null && player.Data.TryGetValue("PlayerName", out var nameData))
                {
                    return nameData.Value;
                }
                else
                {
                    return "Unknown (No PlayerName)";
                }
            }
        }

        return "Unknown (ID Not Found)";
    }
}
