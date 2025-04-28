using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private WandDatabase wandDatabase;
    [SerializeField] private GameObject visuals;
    [SerializeField] private Image characterIconImage;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text IsLockedInText;
    public void UpdateDisplay(CharacterSelectState state)
    {
        print("called Update Display");

        // Character Display
        if (state.CharacterId != -1)
        {
            var character = characterDatabase.GetCharacterById(state.CharacterId);
            if (character == null)
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

        // Name Display (NEW: Grab from LobbyUtils)
        string playerName = state.PLayerDisplayName.ToString();
        if(string.IsNullOrEmpty(playerName))
        {
            playerName = "Unknown Player";
            print(playerName);
        }
        if (state.IsLockedIn)
        {
            IsLockedInText.text = $"(Locked In)";
        }
        else
        {
            IsLockedInText.text = $"(Picking...)";
        }
        playerNameText.text = playerName;

        visuals.SetActive(true);
    }

    public void DisableDisplay()
    {
        visuals.SetActive(false);
    }
}
