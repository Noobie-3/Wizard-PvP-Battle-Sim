using Unity.Netcode;
using UnityEngine;

public class barrier_Spell : NetworkBehaviour , ISpell_Interface
{
    public Spell spell;  // Your spell data object
    public ulong CasterId { get; set; }

    Spell ISpell_Interface.spell => spell;

    public bool Printdata = false;
    public bool hasShotSpell = false;
    public Vector3 Direction;

    private void Start()
    {
        Destroy(gameObject, spell.LifeTime);  // Destroy object after lifetime
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
        //PlaySOund FOr firing
        gameController.GC.PlaySoundAtLocation(transform, spell.FireSound);
        hasShotSpell = true;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;  // The server should handle hits

        if (other.gameObject.GetComponent<ISpell_Interface>() != null)
        {
            var ISpell = other.gameObject.GetComponent<ISpell_Interface>();
            if (ISpell.spell.Spell_Type == spell.Spell_Type)
            {
                return;
            }
            gameController.GC.PlaySoundAtLocation(transform, spell.ImpactSound);
            Destroy(other.gameObject);
            Destroy(gameObject);
        }

    }

    public void TriggerEffect()
    {
        
    }
}
