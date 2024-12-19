using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface ISpell_Interface
{
    Spell spell { get;}
    ulong CasterId { get; set; }
    // bool CanCastSpell();
    float hitagainTime { get; set; }

    void FireSpell();



    void Initialize(ulong casterId, Vector3 direction);


    void TriggerEffect();


}
