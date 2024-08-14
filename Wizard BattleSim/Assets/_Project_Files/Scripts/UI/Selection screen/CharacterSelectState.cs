using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;


public struct CharacterSelectState : INetworkSerializable, IEquatable<CharacterSelectState>
{
    public ulong ClientId;
    public int SelectedCharacterId;


    public CharacterSelectState(ulong clientId, int selectedCharacterId = -1)
    {
        ClientId = clientId;
        SelectedCharacterId = selectedCharacterId;
    }


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref SelectedCharacterId);
    }
    public bool Equals(CharacterSelectState other)
    {
        return ClientId == other.ClientId && SelectedCharacterId == other.SelectedCharacterId;
    }
}
