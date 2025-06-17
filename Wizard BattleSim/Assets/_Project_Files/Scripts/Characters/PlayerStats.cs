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
        if (!IsServer) return;
        if (manaRegenInterval <= 0)
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
            SpendManaServerRpc(amount);
            return true;
        }

        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpendManaServerRpc(float amount)
    {
        Mana.Value -= amount;
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

    [ServerRpc(RequireOwnership = false)]
    public void ChargeManaServerRpc()
    {
        RestoreMana(0.2f);
        var playercontroller = GetComponent<PlayerController>();
        if (playercontroller != null && playercontroller.Charging)
        {
            playercontroller.Anim.SetBool("IsCharging", true);
        }
    }


    public void TakeDamage(float damage, ulong PlayerWhoAttacked)
    {
        TakeDamageServerRpc(damage); // Call the server RPC to handle damage on the server side

        if (Health.Value <= 0)
        {
            Die(PlayerWhoAttacked);
        }
        print("Hp after damage: " + Health.Value + " on " + transform.name);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        Health.Value -= damage;

    }

    private void Die(ulong AttackingPLayer)
    {

        print("called Die On " + transform.name);
        SpawnManager.instance.RespawnPlayerServerRpc(AttackingPLayer);
        SpawnManager.instance.RespawnPlayerServerRpc(this.OwnerClientId);
        WinTracker.Singleton.AddWin(AttackingPLayer);
        // Add respawn or death handling here

        print("Win added");
    }


}




