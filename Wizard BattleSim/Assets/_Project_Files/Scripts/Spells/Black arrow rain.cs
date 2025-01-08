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

    public void SpawnArrows()
    {
        if (!IsOwner)  return;
        SpawnArrowsServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnArrowsServerRpc()
    {
        var Arrows = Instantiate(spell.Grounded_SpellToSpawn_Prefab,PosToSpawn, Quaternion.identity);
        Arrows.GetComponent<Blackarrowrain>().CasterId = CasterId;
        Arrows.GetComponent<NetworkObject>().Spawn();
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

        //Raycast down to spawn arrows on floor
        RaycastHit Rayhit;
        
        if(Physics.Raycast(GroundCheckPos.position,Vector3.down,out Rayhit,Mathf.Infinity))
        {
            PosToSpawn = Rayhit.point;
        }

        SpawnArrows();

    }

    public void TriggerEffect()
    {
        throw new System.NotImplementedException();
    }

}
