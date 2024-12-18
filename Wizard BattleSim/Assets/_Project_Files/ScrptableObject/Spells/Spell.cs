using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spell")]
public class Spell : ScriptableObject {

    public float Spell_Speed; // Spell Travel Speed
    public float Spell_Damage; // Spell's Damage
    public GameObject Spell_Prefab;
    public GameObject Air_SpellToSpawn_Prefab;
    public GameObject Grounded_SpellToSpawn_Prefab;
    public float CooldownDuration = 1; // Cooldown duration in seconds
    public float LifeTime;
    public float ManaCost;
    public string Spell_Name; // Spell Name
    public float Spell_CastTime = 2; // Spell Cast Time
    public float MultiHitCooldown = 0.5f; // Multi Hit Cooldown
    public Sprite SpellIcon;
    public Character CasterChar;
    public AudioClip FireSound;
    public AudioClip ImpactSound;
    public GameObject ImpactEffect;

    public enum SpellType { // The Types Of Spells
        Light, 
        Mid, 
        Heavy
    };
    public SpellType Spell_Type;

    public void SpellHit(GameObject other, GameObject self, ulong CasterId)
    {
        if (other.GetComponent<IHittable_inherited>() || other.GetComponentInChildren<IHittable_inherited>())
        { 
           other.GetComponent<IHittable_inherited>().GotHit(other ,this, CasterId);
        }
    }



}
