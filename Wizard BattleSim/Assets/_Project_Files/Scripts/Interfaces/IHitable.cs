using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitable
{

    void GotHit(GameObject Hitter, Spell spell, GameObject Caster);



}
