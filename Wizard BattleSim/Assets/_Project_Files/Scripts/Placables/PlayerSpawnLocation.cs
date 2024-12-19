using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnLocation : NetworkBehaviour
{

    public NetworkVariable<bool> IsAvailable;

    public void SetAvailability(bool availability)
    {
        IsAvailable.Value = availability;
    }
}


