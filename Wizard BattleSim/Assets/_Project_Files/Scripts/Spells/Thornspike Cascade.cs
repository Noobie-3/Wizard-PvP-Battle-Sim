using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class ThornspikeCascade : NetworkBehaviour, ISpell_Interface {
    public Spell spell;

    public ulong CasterId { get; set; }
    public float hitagainTime { get; set; }
    Spell ISpell_Interface.spell => throw new System.NotImplementedException();

    public Vector3 Direction;
    public float Offset = 5;
    public void FireSpell() {


    }

    public void Initialize(ulong casterId, Vector3 direction) {

        CasterId = casterId;
        Direction = direction;
        transform.LookAt(Direction);
        transform.rotation = new quaternion(0, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        transform.position += transform.forward * Offset;
    }

    public void TriggerEffect() {

    }
}
