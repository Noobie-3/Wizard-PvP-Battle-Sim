using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class FireBall : NetworkBehaviour, ISpell_Interface
{

    [SerializeField] public Spell spell;

    public float CurrentLifeTime;
    public ulong CasterId { get; set; }

    Spell ISpell_Interface.spell => spell;

    public float hitagainTime { get; set; }

    public Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    public void FireSpell()
    {
        if (!IsOwner) return;
        rb.AddForce(spell.Spell_Speed * transform.forward, ForceMode.Impulse);
    }

    public void Initialize(ulong casterId, Vector3 direction)
    {
        CasterId = casterId;
        transform.LookAt(direction);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        IHittable_inherited iHit;

        other.TryGetComponent<IHittable_inherited>(out iHit);
        if (iHit == null) return;
        if (iHit.Type == IHittable_inherited.ObjectType.player && iHit.OwnerClientId == CasterId) return;

        iHit.GotHit(gameObject, spell,  CasterId);

        TriggerEffect();
    }

    public void TriggerEffect()
    {
    }

}
