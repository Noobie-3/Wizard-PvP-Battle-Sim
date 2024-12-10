using NUnit.Framework.Internal.Filters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scroller_InfoButton : MonoBehaviour
{
    public GameObject InfoPanel;
    public TextMeshProUGUI InfoText;
    public Image InfoImage;
    public CharacterDatabase characterDatabase;
    public WandDatabase wandDatabase;
    public SpellBook_AllSpellsList spellDatabase;
    public Scroller_Selector scrollerSelector;
    public Button ConfirmButton;
    public Animation anim;
    public bool PlayAnim = true;
    public Scroller_Selector.SelectionType type;
    public int SpellIndex;
    public bool LockedIn;
    public GameObject SelectorIconHolder;

    public void OnEnable()
    {
        if(PlayAnim)
        {
            anim.Play();
        }
    }

    public void SetInfoPanel(int id)
    {
        if (InfoPanel == null || InfoText == null || InfoImage == null)
        {
            Debug.LogError("InfoPanel, InfoText, or InfoImage is not assigned in the Inspector!");
            return;
        }

        InfoPanel.SetActive(true);

        switch (type)
        {
            case Scroller_Selector.SelectionType.Wand:
                if (wandDatabase == null)
                {
                    Debug.LogError("wandDatabase is not assigned!");
                    return;
                }

                if (id < 0 || id >= wandDatabase.GetAllWands().Length)
                {
                    Debug.LogError($"Invalid Wand ID: {id}. Expected range: 0 to {wandDatabase.GetAllWands().Length - 1}");
                    return;
                }

                InfoText.text = wandDatabase.GetWandById(id).name;
                InfoImage.sprite = wandDatabase.GetWandById(id).Icon;
                print("Wand ID is " + id);
                break;

            case Scroller_Selector.SelectionType.Character:
                if (characterDatabase == null)
                {
                    Debug.LogError("characterDatabase is not assigned!");
                    return;
                }

                if (id < 0 || id >= characterDatabase.GetAllCharacters().Length)
                {
                    Debug.LogError($"Invalid Character ID: {id}. Expected range: 0 to {characterDatabase.GetAllCharacters().Length - 1}");
                    return;
                }

                InfoText.text = characterDatabase.GetCharacterById(id).DisplayName;
                InfoImage.sprite = characterDatabase.GetCharacterById(id).Icon;
                print("Character ID is " + id);
                break;

            case Scroller_Selector.SelectionType.Spell:
                scrollerSelector.SpellIndex = SpellIndex;
                if (spellDatabase == null)
                {
                    Debug.LogError("spellDatabase is not assigned!");
                    return;
                }

                if (id < 0 || id >= spellDatabase.SpellBook.Count)
                {
                    Debug.LogError($"Invalid Spell ID: {id}. Expected range: 0 to {spellDatabase.SpellBook.Count - 1}");
                    return;
                }

                InfoText.text = spellDatabase.SpellBook[id].name;
                InfoImage.sprite = spellDatabase.SpellBook[id].SpellIcon;
                print("Spell ID is " + id);
                break;

            default:
                Debug.LogError($"Unknown SelectionType: {type}");
                InfoText.text = "Unknown Info";
                break;
        }

        print("SetInfoPanel called with type " + type);
    }

}
