using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class Blackarrowrain : NetworkBehaviour, ISpell_Interface
{
    
    [SerializeField] public Spell spell;

    public float CurrentLifeTime;
    [SerializeField] private float MeteorHeight;
    [SerializeField] Coroutine SpawnCoroutine;
    public ulong CasterId { get; set; }

    Spell ISpell_Interface.spell => spell;

    public float hitagainTime { get; set; }

  




    // Update is called once per frame
    void Update()
    {
        CurrentLifeTime += Time.deltaTime;
        if(hitagainTime > 0 )
        {
            hitagainTime -= Time.deltaTime;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        Destroy(gameObject, spell.LifeTime);
    }



    public void FireSpell()
    {
    }

    public void Initialize(ulong casterId, Vector3 direction)
    {
        CasterId = casterId;
        transform.LookAt(direction);



    }



    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;
        IHittable_inherited iHit;

        if(hitagainTime > 0)
        {
            return;
        }
        other.TryGetComponent<IHittable_inherited>(out iHit);
        if (iHit == null) return;
        if (iHit.Type == IHittable_inherited.ObjectType.player && iHit.OwnerClientId == CasterId) return;

        iHit.GotHit(gameObject, spell, CasterId);

        hitagainTime = spell.MultiHitCooldown;


    }

    public void TriggerEffect()
    {
        throw new System.NotImplementedException();
    }

}
