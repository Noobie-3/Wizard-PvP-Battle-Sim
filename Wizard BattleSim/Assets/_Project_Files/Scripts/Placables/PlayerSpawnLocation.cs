using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnLocation : NetworkBehaviour
{

    public bool IsAvailable = true;

    public void SetAvailability(bool availability)
    {
        IsAvailable = availability;
    }
}


