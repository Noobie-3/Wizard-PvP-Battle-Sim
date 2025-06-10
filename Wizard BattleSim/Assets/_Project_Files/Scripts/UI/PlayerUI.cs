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
        }if (healthBar == null) return;        
        if(player == null) return;
        if(manaBar == null) return;
        if(manaText == null) return;

        
        // Convert values to percentages
        float health = player.Stats.Health.Value / player.Stats.MaxHealth.Value;
        float mana = player.Stats.Mana.Value / player.Stats.MaxMana.Value;
        //float stamina = player.Stamina.Value / player.MaxStamina;

        // Fill the bars
        healthBar.fillAmount = health;
       // staminaBar.fillAmount = stamina;
        manaBar.fillAmount = mana;
        
        //round the values to no decimal places
        health = Mathf.Round(health * 100);
        mana = Mathf.Round(mana * 100);
        //stamina = Mathf.Round(stamina * 100);


        // Update the text
        healthText.text = $"{health}/{player.Stats.MaxHealth.Value}";
       // staminaText.text = $"Stamina: {player.Stamina.Value}/{player.MaxStamina}";
        manaText.text = $"{mana}/{player.Stats.MaxMana.Value}";

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
