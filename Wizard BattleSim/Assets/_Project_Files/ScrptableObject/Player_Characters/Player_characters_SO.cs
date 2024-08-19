using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player_characters", menuName = "ScriptableObjects/Player_characters_SO")]
public class Player_characters_SO : ScriptableObject
{

    [SerializeField] private int id = -1;
    [SerializeField] private string characterName = "Character Name";
    [SerializeField] private Sprite characterImage = null;
    [SerializeField ] private GameObject introPrefab;

    public int Id => id; 
    public string CharacterName => characterName;
    public Sprite CharacterImage => characterImage;

    public GameObject IntroPrefab => introPrefab;
}
