using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character_DB", menuName = "ScriptableObjects/Character_DB")]
public class Character_DB : ScriptableObject
{
    [SerializeField] private Player_characters_SO[] characters = new Player_characters_SO[0];

    public Player_characters_SO[] GetAllCharacters() => characters;

    public Player_characters_SO GetCharacter(int id)
    {
        foreach (Player_characters_SO character in characters)
        {
            if (character.Id == id)
            {
                return character;
            }
        }
        return null;
    }

}
