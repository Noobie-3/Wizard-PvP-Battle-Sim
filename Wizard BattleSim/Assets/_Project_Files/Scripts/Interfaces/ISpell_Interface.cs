using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface ISpell_Interface
{
    ulong CasterId { get; set; }

    // bool CanCastSpell();


    void FireSpell();



    void Initialize(ulong casterId, Vector3 direction);





}
