using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    private Player_characters_SO Character;
    private PlayerSelect PlayerSelect;

    public void SetCharacter(Player_characters_SO character, PlayerSelect playerSelect)
    {
        iconImage.sprite = character.CharacterImage;
        this.PlayerSelect = playerSelect;
        this.Character = character;
           
    }

    public void SelectCharacter()
    {
        PlayerSelect.Select(Character);
    }
}
