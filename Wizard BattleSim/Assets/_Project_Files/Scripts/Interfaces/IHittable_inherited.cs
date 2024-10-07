using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IHittable_inherited : NetworkBehaviour, IHitable
{

   [SerializeField] private enum ObjectType {

        nullify,
        Player,
        Breakable

    }
    [SerializeField] ObjectType Type;


    public void GotHit(GameObject ThingThatHitMe, Spell Spell, ulong CasterId ) {
        if(!IsOwner) return;
        if(Type == ObjectType.nullify) {

            Destroy(ThingThatHitMe);
            print("Spell  Nullable obj got hit");

        }    
        
        else if(Type == ObjectType.Player) {


            print("Got hit by a spell" + Spell.Spell_Name + "from" + CasterId);

        }   
        
        else if(Type == ObjectType.Breakable) {
            print("Breakable got hit");

        }

        Destroy(ThingThatHitMe);



    }



}
