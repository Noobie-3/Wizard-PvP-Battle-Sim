using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellBookManager : MonoBehaviour
{
    public static SpellBookManager Singleton;

    SpellBook_AllSpellsList SpellBook;

    private void Start()
    {
        //Singleton pattern
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
    } 
}
