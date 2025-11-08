using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class FireBall : NetworkBehaviour, ISpell_Interface
{
    [SerializeField] public Spell spell;
    public ulong CasterId { get; set; }

    Spell ISpell_Interface.spell => throw new System.NotImplementedException();

    public float hitagainTime { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public Rigidbody rb;

    private bool _hasTriggered;
    private Collider _col;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }

    public void FireSpell()
    {
        if (!IsServer) return;                      // server-owned projectile movement
        rb.AddForce(spell.Spell_Speed * transform.forward, ForceMode.Impulse);
    }

    public void Initialize(ulong casterId, Vector3 lookAt)
    {
        CasterId = casterId;
        transform.LookAt(lookAt);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        IHittable_inherited iHit;


        other.TryGetComponent<IHittable_inherited>(out iHit);
        if (iHit == null) return;
        if (iHit.Type == IHittable_inherited.ObjectType.player && iHit.OwnerClientId == CasterId) return; iHit.GotHit(gameObject, spell, CasterId);
        TriggerEffect();
    }

    private void DespawnProjectile()
    {
        if (TryGetComponent(out NetworkObject no) && no.IsSpawned) no.Despawn(true);
        else Destroy(gameObject);
    }

    public void TriggerEffect()
    {
        DespawnProjectile();
    }
}
