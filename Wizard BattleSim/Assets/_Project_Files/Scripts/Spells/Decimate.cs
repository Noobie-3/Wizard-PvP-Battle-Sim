using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class Decimate : NetworkBehaviour, ISpell_Interface
{

    [SerializeField] public Spell spell;

    public float CurrentLifeTime;
    [SerializeField] Coroutine SpawnCoroutine;
    public ulong CasterId { get; set; }

    Spell ISpell_Interface.spell => spell;

    public float hitagainTime { get; set; }

    public Rigidbody rb;
    public int Hitcount = 0;
    public int MaxHit = 3;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        CurrentLifeTime += Time.deltaTime;
        if (hitagainTime > 0)
        {
            hitagainTime -= Time.deltaTime;
        }
    }



    public void FireSpell()
    {
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

        transform.SetParent(other.transform);


    }

    public void TriggerEffect()
    {
     }

    public IEnumerator DecimateMultiHit(PlayerController PC)
    {
        while (Hitcount < MaxHit)
        {
            Hitcount++;

            PC.TakeDamage(spell, CasterId);
            yield return new WaitForSeconds(spell.MultiHitCooldown);
            
        }
        Destroy(gameObject);
    }
    
}
