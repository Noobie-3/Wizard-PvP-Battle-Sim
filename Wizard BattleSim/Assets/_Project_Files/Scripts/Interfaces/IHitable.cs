using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IHitable
{
    //This function is called when the object is hit
    //it takes in the object that hit it, the spell that hit it, and the object that cast the spell
    void GotHit(GameObject Hitter, Spell spell, ulong Casterid);



}
