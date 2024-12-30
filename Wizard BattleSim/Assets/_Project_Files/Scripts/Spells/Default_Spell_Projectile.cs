using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class Default_Spell_Projectile : NetworkBehaviour, ISpell_Interface
{

    [SerializeField] public Spell spell;

    public float CurrentLifeTime;
    [SerializeField] Coroutine SpawnCoroutine;
    public Vector3 PosToSpawn;
    public ulong CasterId { get; set; }
    public Transform GroundCheckPos;

    Spell ISpell_Interface.spell => spell;

    public float hitagainTime { get; set; }

    public Rigidbody rb;


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

    public void SpawnSpell()
    {
        if (!IsOwner) return;
        SpawnSpellServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnSpellServerRpc()
    {
        var Spell = Instantiate(spell.Grounded_SpellToSpawn_Prefab, PosToSpawn, Quaternion.identity);
        Spell.GetComponent<ISpell_Interface>().CasterId = CasterId;
        Spell.GetComponent<NetworkObject>().Spawn();
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


        PosToSpawn = transform.position;
        //Raycast down to spawn arrows on floor if there is only a ground spell
        RaycastHit Rayhit;

        if (Physics.Raycast(GroundCheckPos.position, Vector3.down, out Rayhit, Mathf.Infinity))
        {
            if(spell.Air_SpellToSpawn_Prefab == null)
            {
                PosToSpawn = Rayhit.point;
            }
        }

        SpawnSpell();

    }

    public void TriggerEffect()
    {
    }

}
