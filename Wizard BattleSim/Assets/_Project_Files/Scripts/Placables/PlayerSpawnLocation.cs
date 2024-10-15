using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnLocation : NetworkBehaviour
{
    public bool CanSpawnPlayer;

    [ClientRpc(RequireOwnership = false)]
    public void SetCanSpawnClientRpc(bool Value)
    {
        CanSpawnPlayer = Value;
    }

}
