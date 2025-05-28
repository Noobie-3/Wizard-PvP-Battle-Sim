using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Netcode;


public struct CharacterSelectState : INetworkSerializable, IEquatable<CharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;
    public int WandID;
    public bool IsLockedIn;
    public int Spell0;
    public int Spell1;
    public int Spell2;
    public FixedString64Bytes PlayerLobbyId; // Use this instead of string
    public FixedString64Bytes PLayerDisplayName; // Use this instead of string
    public int WinCount;
    public int Ranking;

    public CharacterSelectState(ulong clientId, int characterId = -1, int wandID = -1, int spell0 = 0, int spell1 = 1, int spell2 = 2, bool isLockedIn = false, FixedString64Bytes playerLobbyId = default, FixedString64Bytes PlayerName = default, int winCount = 0, int ranking = 0)
    {
        ClientId = clientId;
        CharacterId = characterId;
        WandID = wandID;
        IsLockedIn = isLockedIn;
        Spell0 = spell0;
        Spell1 = spell1;
        Spell2 = spell2;
        PlayerLobbyId = playerLobbyId;
        PLayerDisplayName = PlayerName;
        WinCount = winCount;
        Ranking = ranking;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref WandID);
        serializer.SerializeValue(ref IsLockedIn);
        serializer.SerializeValue(ref Spell0);
        serializer.SerializeValue(ref Spell1);
        serializer.SerializeValue(ref Spell2);
        serializer.SerializeValue(ref PlayerLobbyId);
        serializer.SerializeValue(ref PLayerDisplayName);
        serializer.SerializeValue(ref WinCount);
        serializer.SerializeValue(ref Ranking);
    

    }

    public bool Equals(CharacterSelectState other)
    {
        return ClientId == other.ClientId &&
               CharacterId == other.CharacterId &&
               WandID == other.WandID &&
               IsLockedIn == other.IsLockedIn &&
               Spell0 == other.Spell0 &&
               Spell1 == other.Spell1 &&
               Spell2 == other.Spell2 &&
               PlayerLobbyId.Equals(other.PlayerLobbyId);
    }
}
