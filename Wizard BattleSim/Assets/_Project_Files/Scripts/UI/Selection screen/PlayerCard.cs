using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private Character_DB CharacterDB;
    [SerializeField] private GameObject visuals;
    [SerializeField] private Image CharacterIcon;
    [SerializeField] private TMP_Text PlayerNameText;
    [SerializeField] private TMP_Text CharacterNameText;

    public void UpdateDisplay(CharacterSelectState state)
    {
        if(state.SelectedCharacterId != -1)
        {
            var character = CharacterDB.GetCharacter(state.SelectedCharacterId);
            CharacterIcon.sprite = character.CharacterImage;
            CharacterIcon.enabled = true;
            CharacterNameText.text = character.CharacterName;
        }
        else
        {
            CharacterIcon.enabled = false ;
        }
        PlayerNameText.text = $"Player {state.ClientId}";
        visuals.SetActive(true);
    }

    public void HideDisplay()
    {
        visuals.SetActive(false);
    }
}
