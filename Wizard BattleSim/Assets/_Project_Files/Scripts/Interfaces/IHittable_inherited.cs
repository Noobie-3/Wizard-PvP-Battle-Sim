using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IHittable_inherited : MonoBehaviour, IHitable
{

    private enum ObjectType {

        nullify,
        Player,
        Breakable

    }
    ObjectType Type;


    public void GotHit(GameObject ObjectThatHitMe) {

        if(Type == ObjectType.nullify) {

            Destroy(ObjectThatHitMe);

        }    
        
        else if(Type == ObjectType.Player) {

        }   
        
        else if(Type == ObjectType.Breakable) {

        }





    }



}
