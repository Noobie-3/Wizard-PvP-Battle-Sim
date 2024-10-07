using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MeteorBehavior : NetworkBehaviour
{
    public ulong Caster;
    public Spell spell;

    public void Initialize(ulong caster)
    {
        Caster = caster;
        if(IsServer)
        {
            Destroy(gameObject, spell.LifeTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        if (!IsClient) return;
        NetworkObject networkObject;
        if (other.transform.root.GetComponent<NetworkObject>() == null)
        {
            if (other.transform.root.GetComponentInChildren<NetworkObject>() == null)
            {
                return;
            }
            else
            {
                networkObject = other.transform.root.GetComponentInChildren<NetworkObject>();
            }
        }
        else
        {
            networkObject = other.transform.root.GetComponent<NetworkObject>();
        }


        IHitable ihitable;
        networkObject.TryGetComponent<IHitable>(out ihitable);
        if (ihitable == null)
        {
            ihitable = networkObject.GetComponentInChildren<IHitable>();
        }

        ihitable.GotHit(this.gameObject, spell, Caster); // Call the GotHit method on the object that was hit (if it has one
        // Trigger the spell's effects when it hits something
        TriggerEffect();
    }

    public void TriggerEffect()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }
}
