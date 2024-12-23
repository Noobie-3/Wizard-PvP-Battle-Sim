using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public interface IHitable
{
    //This function is called when the object is hit
    //it takes in the object that hit it, the spell that hit it, and the object that cast the spell
    bool GotHit(GameObject ThingThatHitMe, Spell spell, ulong Casterid);

     public enum ObjectType
    {

        nullify,
        Player,
        Breakable

    }



}
