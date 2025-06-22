using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class ThornspikeCascade : NetworkBehaviour, ISpell_Interface
{
    public Spell spell;

    public ulong CasterId { get; set; }
    public float hitagainTime { get; set; }
    Spell ISpell_Interface.spell => spell;

    public Vector3 Direction;
    public float Offset = 5;
    public float Timer = 0;
    public void FireSpell()
    {


    }

    private void FixedUpdate()
    {
        if(Timer < spell.LifeTime)
        {
            Timer += Time.fixedDeltaTime;
        }
        else
        {
            if (IsServer)
            {
                Destroy(gameObject); // Destroy the Thornspike Cascade spell after its lifetime
                print("Destroying Thornspike Cascade Spell after lifetime " + Timer + " seconds.");
            }
        }
    }

    public void Initialize(ulong casterId, Vector3 direction)
    {

        CasterId = casterId;
        Direction = direction;
        //snap to ground
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 10, Vector3.down, out hit, Mathf.Infinity))
        {
            transform.position = hit.point;
        }
        else
        {
            Debug.LogWarning("Thornspike Cascade: No ground detected below the spell.");
        }


        transform.LookAt(Direction);
        //stop tilting the spell and make it fire flat 
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        transform.position += transform.forward * Offset;
        print("fired Thornspike Cascade from " + CasterId + " in direction " + Direction + " with offset " + Offset);
    }

    public void TriggerEffect()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.gameObject.TryGetComponent(out IHittable_inherited ihit))
        {
            print(ihit.name + "thornSPike  hit this object");
            if (ihit.Type == IHittable_inherited.ObjectType.player)
            {
                ihit.GotHit(this.gameObject, spell, CasterId);
            }
        }
    }
}
