using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MeteorBehavior : NetworkBehaviour, ISpell_Interface
{
    [SerializeField] public Spell spell;

    Spell ISpell_Interface.spell => spell;

    public ulong CasterId { get; set; }
    public float hitagainTime { get; set; }

    public void FireSpell()
    {

    }

    public void Initialize(ulong casterId, Vector3 direction)
    {

    }

    public void TriggerEffect()
    {

    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        Destroy(gameObject, spell.LifeTime);
    }
}
