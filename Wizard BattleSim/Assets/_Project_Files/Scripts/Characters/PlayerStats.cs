using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    public NetworkVariable<float> Mana = new NetworkVariable<float>(100f);
    public NetworkVariable<float> MaxMana = new NetworkVariable<float>(100f);
    public NetworkVariable<float> Health = new NetworkVariable<float>(100f);
    public NetworkVariable<float> MaxHealth = new NetworkVariable<float>(100f);
    [SerializeField] public Character CharacterChosen;
    // mana regen over time
    [SerializeField] private float manaRegenAmount = 5f;
    [SerializeField] private float manaRegenInterval = 1f;
    private void Update()
    {
        if (!IsOwner) return;
        if(manaRegenInterval <= 0 )
        {
           RestoreMana(manaRegenAmount);
            manaRegenInterval = 1f; // Reset the interval
        }
        else
        {
            manaRegenInterval -= Time.deltaTime; // Decrease the interval
        }

    }
    public bool SpendMana(float amount)
    {
        if (Mana.Value >= amount)
        {
            Mana.Value -= amount;
            return true;
        }

        return false;
    }

    public void RestoreMana(float amount)
    {
        if (Mana.Value >= MaxMana.Value)
        {
            Mana.Value = MaxMana.Value; // Prevents overfilling mana
            return; // Prevents overfilling mana 
        }
        Mana.Value = Mathf.Min(Mana.Value + amount, MaxMana.Value);// Fills mana otherwise
    }

    public void ChargeMana()
    {
        RestoreMana(0.2f);
    }

    public void TakeDamage(float damage, ulong PLayerWhoAttacked)
    {
        Health.Value -= damage;
        if (Health.Value <= 0)
        {
            Die(PLayerWhoAttacked);
            print("called Die On " + CharacterChosen.DisplayName);
            SpawnManager.instance.RespawnPlayerServerRpc(PLayerWhoAttacked);
            SpawnManager.instance.RespawnPlayerServerRpc(this.OwnerClientId);
        }
    }

    private void Die(ulong AttackingPLayer)
    {
        Debug.Log("Player died!");
        // Add respawn or death handling here
        if (WinTracker.Singleton != null)
        {
            for (int i = 0; i < gameController.GC.Players.Length; i++)
            {
                if (gameController.GC.Players[i].OwnerClientId == AttackingPLayer)
                {
                    WinTracker.Singleton.AddWin(AttackingPLayer);
                    print("Win added");
                    break;
                }
            }

            print("Win added");
        }


    }


}
