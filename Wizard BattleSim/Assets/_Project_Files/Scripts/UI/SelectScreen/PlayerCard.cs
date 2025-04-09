using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private WandDatabase WandDatabase;
    [SerializeField] private GameObject visuals;
    [SerializeField] private Image characterIconImage;
    [SerializeField] private TMP_Text playerNameText;
    private float timer = 0;

    public void UpdateDisplay(CharacterSelectState state)
    {
        if(timer  < 0.5f)
        {
            timer += Time.deltaTime;
            return;
        }

        if (state.CharacterId != -1)
        {
            var character = characterDatabase.GetCharacterById(state.CharacterId);
            if(character == null)
            {
                Debug.LogError($"Character with ID {state.CharacterId} not found in database");
                return;
            }
            characterIconImage.sprite = character.Icon;
            characterIconImage.enabled = true;
        }
        else
        {
            characterIconImage.enabled = false;
        }

        if(state.WandID != -1)
        {
            var wand = WandDatabase.GetWandById(state.WandID);
            if(wand == null)
            {
                Debug.LogError($"Wand with ID {state.WandID} not found in database");
                return;
            }
        }
        

        playerNameText.text = state.IsLockedIn ? $"Player {state.ClientId}" : $"Player {state.ClientId} (Picking...)";

        visuals.SetActive(true);
    }

    public void DisableDisplay()
    {
        visuals.SetActive(false);
    }
}
