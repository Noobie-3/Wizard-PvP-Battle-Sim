using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class First_Spell_Test_DELETELATER : NetworkBehaviour, ISpell_Interface
{
    public Spell Curernt_spell;
    public Rigidbody Rb;
    public GameObject Caster { get; set; }

    private void Start()
    {
        if (!IsOwner) return;
        if (!IsServer) return;
        Rb = GetComponent<Rigidbody>();
        DestroyObjectServerRpc(Curernt_spell.LifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // Check to see if the object is hittable
        if (other.gameObject.GetComponent<PlayerController>() != null)
        {
            // If not caster
            if (other.gameObject == Caster) return;

            // If the object is hittable
            Debug.Log("Hit someone besides the caster");
            if (other.gameObject.GetComponent<IHitable>() == null) return;

            other.gameObject.GetComponent<PlayerController>().GotHit(gameObject, Curernt_spell, Caster);

            Debug.Log("Hit someone besides the caster and should do damage");

            // Destroy the object
            DestroyObjectServerRpc(0);
        }
    }

    [ServerRpc]
    void HandleHitServerRpc(ulong targetNetworkObjectId, float damage)
    {
        NetworkObject targetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        if (targetObject != null)
        {
            var hittable = targetObject.GetComponent<IHitable>();
            if (hittable != null)
            {
                hittable.GotHit(gameObject, Curernt_spell, Caster);
                print("Supposed to call gothit");
            }
            else
            {
                Debug.Log("Object hit is not hittable");
            }
        }
    }

    // Server rpc to destroy the object
    [ServerRpc]
    void DestroyObjectServerRpc(float time)
    {
        Destroy(gameObject, time);
    }
}
