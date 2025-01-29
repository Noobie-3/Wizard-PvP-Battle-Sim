using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class IHittable_inherited : NetworkBehaviour, IHitable
{

    [SerializeField]
    public enum ObjectType
    {
        player,
        nullify,
        Breakable

    }
    [SerializeField] public ObjectType Type;
    [SerializeField] public PlayerController PC;



    public bool GotHit(GameObject ThingThatHitMe, Spell Spell, ulong CasterId)
    {
        if(!IsServer) return false;
        if (ThingThatHitMe == null)
        {
            Debug.LogWarning("Spell hit null object.");
            return true;
        }

        if (Spell == null)
        {
            Debug.LogWarning("Spell is null.");
            return false;
        }

        Debug.Log(CasterId + " is the Caster ID. Spell hit.");

        switch (Type)
        {
            case ObjectType.nullify:
                Debug.Log("Nullable object got hit: " + gameObject.name);
                break;

            case ObjectType.player:
                Debug.Log(OwnerClientId + "owner and casterId " + CasterId);
                if (OwnerClientId == CasterId) return false;
                Debug.Log("Player got hit.");
                PC.TakeDamage(Spell, CasterId);
                return true;

            case ObjectType.Breakable:
                Debug.Log("Breakable object got hit.");
                return true;
        }
        return false;
    }

    
}