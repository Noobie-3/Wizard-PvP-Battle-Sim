using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface ISpell_Interface
{
    NetworkObject Caster { get; set; }
}
