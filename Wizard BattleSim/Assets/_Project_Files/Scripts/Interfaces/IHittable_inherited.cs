using Unity.Netcode;
using UnityEngine;

public class IHittable_inherited : NetworkBehaviour, IHitable
{
    public enum ObjectType { player, nullify, Breakable }
    [SerializeField] public ObjectType Type = ObjectType.player;
    [SerializeField] public PlayerController PC;

    private void Awake()
    {
        if (!PC) PC = GetComponent<PlayerController>() ?? GetComponentInParent<PlayerController>();
    }

    // Server-only entry point
    public bool GotHit(GameObject thingThatHitMe, Spell spell, ulong casterId)
    {
        if (!IsServer) return false;
        if (spell == null) return false;

        switch (Type)
        {
            case ObjectType.nullify:
                return true;

            case ObjectType.player:
                if (OwnerClientId == casterId) return false; // self-hit protection
                if (!PC) return false;

                // Call server damage path directly (no client decision making)
                if(spell.MultiHitCooldown <= 0)
                {
                    Destroy(thingThatHitMe);
                }
                PC.TakeDamageServerRPC(spell.Spell_Damage, casterId);//untested change may revert
                return true;

            case ObjectType.Breakable:
                // TODO: breakable logic here
                return true;
        }
        return false;
    }
}
