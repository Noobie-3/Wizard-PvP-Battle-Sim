using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public PlayerController player;
    public SpellCaster SpellCaster;

    public Image healthBar;
    public TextMeshProUGUI healthText;
    public Image staminaBar;
    public TextMeshProUGUI staminaText;
    public Image manaBar;
    public TextMeshProUGUI manaText;
    public Image[] SpellSlotsImages;
    public GameObject CurrentIcon;
    public GameObject CurrentIconEffect;
    public int CurrentIconIndex;

    // Start is called before the first frame update
    void Start()
    {
        CurrentIcon = Instantiate(CurrentIconEffect, SpellSlotsImages[SpellCaster.SelectedSpell].transform);
        if(player == null)
        {
            player = GetComponentInParent<PlayerController>();
        }

        

    }

    // Update is called once per frame


    public void UpdateUI()
    {

        UpdateSelection();
        for (int i = 0; i < SpellCaster.MaxSpells; i++)
        {
            if(SpellSlotsImages[i].sprite != SpellCaster.SpellBook.SpellBook[SpellCaster.CurrentSpells[i]].SpellIcon)
            {
                SpellSlotsImages[i].sprite = SpellCaster.SpellBook.SpellBook[SpellCaster.CurrentSpells[i]].SpellIcon;
            }
        }
        
        // Convert values to percentages
        float health = player.Health.Value / player.MaxHealth;
        float mana = player.Mana.Value / player.MaxMana;
        float stamina = player.Stamina.Value / player.MaxStamina;

        // Fill the bars
        healthBar.fillAmount = health;
        staminaBar.fillAmount = stamina;
        manaBar.fillAmount = mana;

        // Update the text
        healthText.text = $"Health: {player.Health.Value}/{player.MaxHealth}";
        staminaText.text = $"Stamina: {player.Stamina.Value}/{player.MaxStamina}";
        manaText.text = $"Mana: {player.Mana.Value}/{player.MaxMana}";

        //Change Spell icons for selection


    }

    public void UpdateSelection()
    {
        if (CurrentIconIndex == SpellCaster.SelectedSpell) return;
        if(CurrentIcon)
        {
            Destroy(CurrentIcon);
        }
        CurrentIcon = Instantiate(CurrentIconEffect, SpellSlotsImages[SpellCaster.SelectedSpell].transform);
        CurrentIconIndex = SpellCaster.SelectedSpell;
    }



}
