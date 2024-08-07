using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    public PlayerController player;
    public Image healthBar;
    public TextMeshProUGUI healthText;
    public Image staminaBar;
    public TextMeshProUGUI staminaText;
    public Image manaBar;
    public TextMeshProUGUI manaText;


    // Start is called before the first frame update
    void Start()
    {
        if(player == null)
        {
            player = GetComponentInParent<PlayerController>();
        }
        if (!IsOwner) 
        {
            this.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!IsOwner) return;
        // Convert values to percentages
        float health = player.Health.Value / player.MaxHealth.Value;
        float mana = player.Mana.Value / player.MaxMana.Value;
        float stamina = player.Stamina.Value / player.MaxStamina.Value;

        // Fill the bars
        healthBar.fillAmount = health;
        staminaBar.fillAmount = stamina;
        manaBar.fillAmount = mana;

        // Update the text
        healthText.text = $"Health: {player.Health.Value}/{player.MaxHealth.Value}";
        staminaText.text = $"Stamina: {player.Stamina.Value}/{player.MaxStamina.Value}";
        manaText.text = $"Mana: {player.Mana.Value}/{player.MaxMana.Value}";
    }


}
