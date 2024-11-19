using System;
using Unity.Netcode;

public struct CharacterSelectState : INetworkSerializable, IEquatable<CharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;
    public int WandID;
    public bool IsLockedIn;


    public CharacterSelectState(ulong clientId, int characterId = -1, int wandID = 0, bool isLockedIn = false)
    {
        ClientId = clientId;
        CharacterId = characterId;
        WandID = wandID;
        IsLockedIn = isLockedIn;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref WandID);
        serializer.SerializeValue(ref IsLockedIn);
    }

    public bool Equals(CharacterSelectState other)
    {
        return ClientId == other.ClientId &&
            CharacterId == other.CharacterId &&
            WandID == other.WandID &&
            IsLockedIn == other.IsLockedIn;
    }
}
