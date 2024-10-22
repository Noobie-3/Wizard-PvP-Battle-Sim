using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IHittable_inherited : NetworkBehaviour, IHitable
{

   [SerializeField] private enum ObjectType {
        player,
        nullify,
        Breakable

    }
    [SerializeField] ObjectType Type;
    [SerializeField] private PlayerController PC;

    public void GotHit(GameObject ThingThatHitMe, Spell Spell, ulong CasterId ) {
        if (Type == ObjectType.nullify)
        {

            print("Spell  Nullable obj got hit");
            print(gameObject.name);

        }
        else if(Type == ObjectType.player) {
            print("Player got hit");
            PC = GetComponent<PlayerController>();
            PC.TakeDamage(Spell); 
        }

        else if(Type == ObjectType.Breakable) {
            print("Breakable got hit");

        }





    }


    /*private void OnTriggerEnter(Collider other)
    {
        ISpell_Interface ISpell;
        other.gameObject.TryGetComponent<ISpell_Interface>(out ISpell);

        if(ISpell != null) 
        {
            GotHit(other.gameObject, ISpell.spell, ISpell.CasterId);
            ISpell.TriggerEffect();

        }
    }*/



}
