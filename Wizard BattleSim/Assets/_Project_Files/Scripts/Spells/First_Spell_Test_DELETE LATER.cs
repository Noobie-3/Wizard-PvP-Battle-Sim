using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class First_Spell_Test_DELETELATER : NetworkBehaviour, ISpell_Interface
{
    public Spell spell;  // Your spell data object
    public Rigidbody Rb;
    public ulong CasterId { get; set; }

    Spell ISpell_Interface.spell => spell;

    public bool Printdata = false;
    public bool hasShotSpell = false;
    public Vector3 Direction;

    private void Start()
    {
        if (IsOwner)
        {

            //Destroy(gameObject, spell.LifeTime);

            //DestroyObjectServerRpc(spell.LifeTime);  // Destroy object after lifetime on the server
        }
    }

    // This method is called by the player to initialize the spell
    public void Initialize(ulong casterId, Vector3 direction)
    {
        Rb = GetComponent<Rigidbody>();
        CasterId = casterId;  // Set the caster's ID
        Direction = direction; // Set the movement direction

        print("The spell says the caster is " + CasterId);
    }
    
    public void FireSpell()
    {
        if (!hasShotSpell)
        {
            transform.LookAt(Direction);
            Rb.AddForce(Direction * spell.Spell_Speed, ForceMode.Impulse);
            print("Fired the spell");
            hasShotSpell = true;
        }
    }




    // Server RPC to destroy the object after a set amount of time
/*    [ServerRpc(RequireOwnership = false)]
    private void DestroyObjectServerRpc(float time)
    {
        print("SpellDied");
        Destroy(gameObject, time);
    }*/

    private void OnTriggerStay(Collider other)
    {
/*        NetworkObject networkObject;
        if(other.transform.root.GetComponent<NetworkObject>() == null)
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

        ihitable.GotHit(this.gameObject, spell, CasterId); // Call the GotHit method on the object that was hit (if it has one
        // Trigger the spell's effects when it hits something
        TriggerEffect();*/


    }

    // Method to trigger any spell effects (like damage or visual effects)
    public void TriggerEffect()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
        print("SpellTriggered effect");

        

        // Add custom effects here, such as damage or explosions

    }

    public IEnumerator PrintData()
    {
        while (Printdata)
        {
            if (spell == null)
            {
                print("Spell is null");
            }

            if (Rb == null)
            {
                print("Rb is null");
            }

            yield return 1f;
        }
        yield return null;
    }
}
