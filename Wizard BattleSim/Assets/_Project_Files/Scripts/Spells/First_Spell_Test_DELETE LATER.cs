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
            Rb = GetComponent<Rigidbody>();  // Ensuring the Rigidbody is set
            DestroyObjectServerRpc(spell.LifeTime);  // Destroy object after lifetime on the server
        }
    }

    // This method is called by the player to initialize the spell
    public void Initialize(ulong casterId, Vector3 direction)
    {
        CasterId = casterId;  // Set the caster's ID
        Direction = direction; // Set the movement direction
        print("The spell says the caster is " + CasterId);
    }

    public void FireSpell()
    {

/*        transform.LookAt(Direction);
        Rb.AddForce(transform.forward * spell.Spell_Speed, ForceMode.Impulse);
        Debug.Log("Fired the spell in the direction: " + Direction);
        hasShotSpell = true;
        */
    }




    // Server RPC to destroy the object after a set amount of time
    [ServerRpc(RequireOwnership = false)]
    private void DestroyObjectServerRpc(float time)
    {
        print("SpellDied");
        Destroy(gameObject, time);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;  // The server should handle hits

        IHitable ihitable = other.transform.root.GetComponent<IHitable>();
        if (ihitable == null)
        {
            Debug.LogWarning("No IHitable found on " + other.gameObject.name);
            return;
        }

        Debug.Log($"Spell {gameObject.name} hit {other.gameObject.name} with spell {spell}. Caster ID: {CasterId}");

        var HitData = ihitable.GotHit(gameObject, spell, CasterId);// Notify the hit object
        if (HitData == false) return;
        TriggerEffect();  // Trigger spell effects
    }

    // Method to trigger any spell effects (like damage or visual effects)
    public void TriggerEffect()
    {
        // Trigger VFX/SFX here before destroying the object
        // Example: Instantiate an explosion effect, or play a sound effect
        DestroyObjectServerRpc(.1f);

        Debug.Log("Spell triggered effect and is being destroyed.");
    }

    public IEnumerator PrintData()
    {
        while (Printdata)
        {
            if (spell == null)
            {
                Debug.LogError("Spell is null");
            }

            if (Rb == null)
            {
                Debug.LogError("Rigidbody is null");
            }

            yield return new WaitForSeconds(1f);  // Wait 1 second between logs
        }
    }
    private void FixedUpdate()
    {
        transform.Translate(Direction * spell.Spell_Speed * Time.deltaTime);

    }
}
