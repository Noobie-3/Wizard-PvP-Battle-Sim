using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class First_Spell_Test_DELETELATER : NetworkBehaviour, ISpell_Interface
{
    public Spell Curernt_spell;  // Your spell data object
    public Rigidbody Rb;
    public ulong CasterId { get; set; }
    public bool Printdata = false;
    public bool hasShotSpell = false;
    public Vector3 Direction;

    private void Start()
    {


        if (IsServer)
        {
            DestroyObjectServerRpc(Curernt_spell.LifeTime);  // Destroy object after lifetime on the server
        }
    }

    // This method is called by the player to initialize the spell
    public void Initialize(ulong casterId, Vector3 direction)
    {
        Rb = GetComponent<Rigidbody>();
        this.CasterId = casterId;  // Set the caster's ID
        this.Direction = direction; // Set the movement direction
    }
    
    public void FireSpell()
    {
        Rb.AddForce(Direction * Curernt_spell.Spell_Speed, ForceMode.Impulse);
        print("Fired the spell");
    }




    // Server RPC to destroy the object after a set amount of time
    [ServerRpc(RequireOwnership = false)]
    private void DestroyObjectServerRpc(float time)
    {
        Destroy(gameObject, time);
    }

    private void OnTriggerEnter(Collider collision)
    {
        NetworkObject networkObject;
        if(collision.transform.root.GetComponent<NetworkObject>() == null)
        {
            if (collision.transform.root.GetComponentInChildren<NetworkObject>() == null)
            {
                return;
            }
            else
            {
                networkObject = collision.transform.root.GetComponentInChildren<NetworkObject>();
            }
        }
        else
        {
            networkObject = collision.transform.root.GetComponent<NetworkObject>();
        }
        // Ignore collision with the caster
        if (networkObject.NetworkObjectId == CasterId)
        {
            return; // Do nothing if the collision is with the caster
        }

        IHitable ihitable;
        networkObject.TryGetComponent<IHitable>(out ihitable);
        if (ihitable == null)
        {
            ihitable = networkObject.GetComponentInChildren<IHitable>();
        }



        // Trigger the spell's effects when it hits something
        TriggerEffect(ihitable);

        // Destroy the spell after the collision (on the server)
        if (IsServer)
        {
            Destroy(gameObject); // This will sync with clients due to network synchronization
        }
    }

    // Method to trigger any spell effects (like damage or visual effects)
    private void TriggerEffect(IHitable IHitable)
    {
        if(IHitable == null)
        {
            return;
        }
        IHitable.GotHit(gameObject, Curernt_spell, CasterId);  // Call the GotHit method on the object that was hit
        // Add custom effects here, such as damage or explosions

    }

    public IEnumerator PrintData()
    {
        while (Printdata)
        {
            if (Curernt_spell == null)
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
