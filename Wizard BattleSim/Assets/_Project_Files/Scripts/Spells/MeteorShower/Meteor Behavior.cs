using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MeteorBehavior : NetworkBehaviour, ISpell_Interface
{
    [SerializeField] public Spell spell;

    Spell ISpell_Interface.spell => spell;

    public ulong CasterId { get; set; }
    public float hitagainTime { get; set; }
    public GameObject Parent;
    public GameObject ImpactEffect;
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
        Destroy(Parent, 25);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.gameObject.TryGetComponent(out IHittable_inherited ihit))
        {
            print(ihit.name + "meteor hit this object");
            if (ihit.Type == IHittable_inherited.ObjectType.player)
            {
                ihit.GotHit(this.gameObject, spell, CasterId);
                

            }
            var TempLocation = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            var Impact = Instantiate(ImpactEffect, TempLocation, Quaternion.identity);
            if (Impact != null)
            {
                gameController.GC.DestroyObjectOnNetwork(Impact, 5);
            }
            Impact.GetComponent<NetworkObject>().Spawn();
            Destroy(gameObject);



        }
    }
}
