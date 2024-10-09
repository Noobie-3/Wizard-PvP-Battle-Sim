using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
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


    // Start is called before the first frame update
    void Start()
    {
        if(player == null)
        {
            player = GetComponentInParent<PlayerController>();
        }

    }

    // Update is called once per frame


    public void UpdateUI()
    {
        for (int i = 0; i < SpellCaster.MaxSpells; i++)
        {
            if(SpellSlotsImages[i].sprite != SpellCaster.SpellBook.SpellBook[SpellCaster.CurrentSpells[i]].SpellIcon)
            {
                SpellSlotsImages[i].sprite = SpellCaster.SpellBook.SpellBook[SpellCaster.CurrentSpells[i]].SpellIcon;
            }
        }
        
        // Convert values to percentages
        float health = player.Health / player.MaxHealth;
        float mana = player.Mana / player.MaxMana;
        float stamina = player.Stamina / player.MaxStamina;

        // Fill the bars
        healthBar.fillAmount = health;
        staminaBar.fillAmount = stamina;
        manaBar.fillAmount = mana;

        // Update the text
        healthText.text = $"Health: {player.Health}/{player.MaxHealth}";
        staminaText.text = $"Stamina: {player.Stamina}/{player.MaxStamina}";
        manaText.text = $"Mana: {player.Mana}/{player.MaxMana}";

        //Change Spell icons for selection


    }


}
