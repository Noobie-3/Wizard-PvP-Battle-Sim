using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Collections.Generic;

public class DevMenu : MonoBehaviour
{
    public Dropdown typeDropdown;         // Dropdown to select SO type
    public Dropdown objectDropdown;       // Dropdown to select specific SO instance
    public GameObject fieldParent;        // Parent for dynamically created fields
    public GameObject inputFieldPrefab;   // Prefab for input fields
    public CharacterDatabase AllCharacters; // Reference to the manager that stores SOs
    public SpellBook_AllSpellsList spellBookManager;

    private ScriptableObject selectedSO;

    void Start()
    {
        // Populate type dropdown with available SO types
        typeDropdown.options.Clear();
        typeDropdown.options.Add(new Dropdown.OptionData("Character"));
        typeDropdown.options.Add(new Dropdown.OptionData("Weapon"));

        // Add listener for type selection
        typeDropdown.onValueChanged.AddListener(OnTypeSelected);
        objectDropdown.onValueChanged.AddListener(OnObjectSelected);
    }

    void OnTypeSelected(int index)
    {
        // Clear previous object options and field UI
        objectDropdown.ClearOptions();
        ClearFields();

        // Populate object dropdown based on selected type
        if (index == 0)  // Character selected
        {
            foreach (Character character in AllCharacters.characters)
            {
                objectDropdown.options.Add(new Dropdown.OptionData(character.name));
            }
        }
        else if (index == 1)  // Weapon selected
        {
            foreach (Spell spell in spellBookManager.SpellBook)
            {
                objectDropdown.options.Add(new Dropdown.OptionData(spell.name));
            }
        }

        objectDropdown.RefreshShownValue();
        OnObjectSelected(0); // Update fields based on first selection
    }

    void OnObjectSelected(int index)
    {
        ClearFields();

        // Select the correct SO from the manager
        if (typeDropdown.value == 0 && index < AllCharacters.characters.Length)
        {
            selectedSO = AllCharacters.characters[index];
        }
        else if (typeDropdown.value == 1 && index < spellBookManager.SpellBook.Count)
        {
            selectedSO = spellBookManager.SpellBook[index];
        }

        if (selectedSO != null)
        {
            CreateFieldsForSO(selectedSO);
        }
    }

    void CreateFieldsForSO(ScriptableObject so)
    {
        // Use reflection to get all fields of the SO and dynamically create UI for them
        FieldInfo[] fields = so.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == typeof(int))
            {
                // Instantiate a new input field UI element
                GameObject fieldUI = Instantiate(inputFieldPrefab, fieldParent.transform);
                InputField inputField = fieldUI.GetComponent<InputField>();

                // Set the label and initial value
                Text label = fieldUI.transform.Find("Label").GetComponent<Text>();
                label.text = field.Name;
                inputField.text = field.GetValue(so).ToString();

                // Add listener to update SO when value is changed
                inputField.onEndEdit.AddListener(value => field.SetValue(so, int.Parse(value)));
            }
            else if (field.FieldType == typeof(string))
            {
                GameObject fieldUI = Instantiate(inputFieldPrefab, fieldParent.transform);
                InputField inputField = fieldUI.GetComponent<InputField>();

                Text label = fieldUI.transform.Find("Label").GetComponent<Text>();
                label.text = field.Name;
                inputField.text = (string)field.GetValue(so);

                inputField.onEndEdit.AddListener(value => field.SetValue(so, value));
            }
            // Add other types as needed (float, bool, etc.)
        }
    }

    void ClearFields()
    {
        foreach (Transform child in fieldParent.transform)
        {
            Destroy(child.gameObject);  // Clear the UI fields before generating new ones
        }
    }
}
