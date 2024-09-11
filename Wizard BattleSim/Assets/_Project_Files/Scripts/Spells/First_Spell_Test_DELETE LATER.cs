using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class First_Spell_Test_DELETELATER : NetworkBehaviour, ISpell_Interface
{
    public Spell Curernt_spell;
    public Rigidbody Rb;
    public NetworkObject Caster { get; set; }
    public bool Printdata = true;

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
            if (other.GetComponent<NetworkObject>() == Caster) return;

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

    public override void OnNetworkSpawn()
    {
        if(Caster == null)
        {
            print("Caster is null");
        }
        else
        {
            var Player = Caster.gameObject.transform.root.GetComponent<PlayerController>();
            if(Player == null) {
                print("Player is null");
            }
            else {
                print("Player is " + Player.name);
            }

            if (Curernt_spell == null) {
                print("Spell is null");
            }
            else {
                print("Spell is " + Curernt_spell.Spell_Name);
            }
            Rb.AddForce(Player.CameraRotation * Curernt_spell.Spell_Speed);
            print("Shot in the dir of the camera" + Player.CameraRotation+ " the speed is " + Curernt_spell.Spell_Speed);

        }
    }


    public IEnumerator PrintData() {
        while (Printdata) {

            print("The spell is " + Curernt_spell.Spell_Name + " the speed is " + Curernt_spell.Spell_Speed + " the damage is " + Curernt_spell.Spell_Damage + "caster is " + Caster.name);

            yield return 5f;
        }
        yield return null;
    }
}
