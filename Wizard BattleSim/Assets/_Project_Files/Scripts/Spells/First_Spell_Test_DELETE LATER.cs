using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]

public class First_Spell_Test_DELETELATER : MonoBehaviour
{
    public Spell Curernt_spell;
    public Rigidbody Rb;


    private void Start() {

        Rb = GetComponent<Rigidbody>();
        Destroy(gameObject, Curernt_spell.LifeTime);
    }


    private void Update() {

    }


}
