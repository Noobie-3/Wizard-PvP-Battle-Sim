using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PoseidonWrath: NetworkBehaviour, ISpell_Interface
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
        if(hitagainTime > 0 )
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
        //snap to ground
        RaycastHit hit;
        if (Physics.Raycast(transform.position , Vector3.down, out hit, Mathf.Infinity))
        {
            transform.position = hit.point;
            print("Poseidons Wrath: Spell snapped to ground at position " + hit.point + " from caster " + CasterId);
        }
        else
        {
            Debug.LogWarning("POsidions WRath: No ground detected below the spell.");
        }


        //stop tilting the spell and make it fire flat 
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);



    }

    public override void OnNetworkSpawn()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        IHittable_inherited iHit;

        other.TryGetComponent<IHittable_inherited>(out iHit);
        if (iHit == null) return;
        if (iHit.Type == IHittable_inherited.ObjectType.player && iHit.OwnerClientId == CasterId) return;

        if(iHit.Type != IHittable_inherited.ObjectType.player) 
        {
            return;
        }
        iHit.GotHit(this.gameObject,spell,CasterId);
        Destroy(gameObject); // Destroy the spell after it hits something

    }

    public void TriggerEffect()
    {
        throw new System.NotImplementedException();
    }

}
