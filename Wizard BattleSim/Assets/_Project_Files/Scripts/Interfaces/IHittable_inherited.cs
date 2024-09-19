using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IHittable_inherited : MonoBehaviour, IHitable
{

   [SerializeField] private enum ObjectType {

        nullify,
        Player,
        Breakable

    }
    [SerializeField] ObjectType Type;


    public void GotHit(GameObject ObjectThatHitMe, Spell Spell, ulong CasterId ) {
        
        if(Type == ObjectType.nullify) {

            Destroy(ObjectThatHitMe);
            print("Spell  Nullable obj got hit");

        }    
        
        else if(Type == ObjectType.Player) {


            print("Got hit by a spell" + Spell.Spell_Name + "from" + CasterId);

        }   
        
        else if(Type == ObjectType.Breakable) {
            print("Breakable got hit");

        }





    }



}
