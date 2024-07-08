using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]

public class First_Spell_Test_DELETELATER : MonoBehaviour, Spell_Interface
{
    public Spell Curernt_spell;
    public Rigidbody Rb;
    public GameObject Caster_;

    private void Start() {

        Rb = GetComponent<Rigidbody>();
        Destroy(gameObject, Curernt_spell.LifeTime);
    }


    private void OnTriggerEnter(Collider other)
    {
        //check to see if object has Ihitable interface
        //check to make sure ibject is not caster of spell
        if (other.GetComponent<IHittable_inherited>() || other.GetComponentInChildren<IHittable_inherited>() && other.gameObject.GetComponent<PlayerController>())
        {
            //if it does, call the GotHit method
            other.GetComponent<IHittable_inherited>().GotHit(gameObject);
        }
    }

}
