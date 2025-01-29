using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spell")]
public class Spell : ScriptableObject {


    [Header("Spell General Info")]
    [Tooltip("The name of the spell displayed to players.")]
    public string Spell_Name; // Spell Name

    [Tooltip("The icon representing the spell in the UI.")]
    public Sprite SpellIcon;

    [Tooltip("The mana cost required to cast this spell.")]
    public float ManaCost;

    [Tooltip("The cooldown duration before the spell can be cast again (in seconds).")]
    public float CooldownDuration = 1; // Cooldown duration in seconds

    [Tooltip("The time it takes to cast the spell (in seconds).")]
    public float Spell_CastTime = 2; // Spell Cast Time

    [Tooltip("The Client id of who cast the spell")]
    public Character CasterChar;

    [Header("Spell Mechanics")]
    [Tooltip("The speed at which the spell travels.")]
    public float Spell_Speed; // Spell Travel Speed

    [Tooltip("The damage dealt by the spell.")]
    public float Spell_Damage; // Spell's Damage

    [Tooltip("The duration (in seconds) before the spell disappears.")]
    public float LifeTime;


    [Tooltip("The cooldown time (in seconds) between multi-hits for the spell.")]
    public float MultiHitCooldown = 0.5f; // Multi Hit Cooldown

    [Header("Spell Prefabs")]
    [Tooltip("The main prefab representing the spell.")]
    public GameObject Spell_Prefab;

    [Tooltip("The prefab for the air version of the spell.")]
    public GameObject Air_SpellToSpawn_Prefab;

    [Tooltip("The prefab for the grounded version of the spell.")]
    public GameObject Grounded_SpellToSpawn_Prefab;

    [Header("Spawnable Effects")]
    [Tooltip("The visual effect triggered when the spell impacts something.")]
    public GameObject ImpactEffect;

    [Header("Sounds")]
    [Tooltip("The sound effect played when the spell is fired.")]
    public AudioClip FireSound;

    [Tooltip("The sound effect played when the spell impacts something.")]
    public AudioClip ImpactSound;

    [Tooltip("The Sound Volume")]
    [Range(0, 1)]
    public float SoundVolume = 1;

    [Tooltip("The Sound Priority")]
    [Range(0,256)]
    public  int SoundPriority = 128;

    [Tooltip("The Sound Pitch")]
    [Range(-3,3)]
    public int SoundPitch = 1;


    [Header("Type of spell")]
    [Tooltip("The type of the spell: Light, Mid, or Heavy.")]

    public SpellType Spell_Type;
    public enum SpellType
    {
        Light,
        Mid,
        Heavy
    }





    public void SpellHit(GameObject other, GameObject self, ulong CasterId)
    {
        if (other.GetComponent<IHittable_inherited>() || other.GetComponentInChildren<IHittable_inherited>())
        { 
           other.GetComponent<IHittable_inherited>().GotHit(other ,this, CasterId);
        }
    }



}
