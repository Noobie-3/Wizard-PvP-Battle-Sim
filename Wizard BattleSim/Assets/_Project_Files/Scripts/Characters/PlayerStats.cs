using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

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
    [SerializeField] private bool _isDead = false;
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
        TakeDamageServerRpc(damage, PlayerWhoAttacked); // Call the server RPC to handle damage on the server side

        print("Hp after damage: " + Health.Value + " on " + transform.name);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, ulong playerWhoAttacked)
    {

        Health.Value -= damage;

        if (Health.Value <= 0f)
        {
            _isDead = true;
            ServerDie(playerWhoAttacked);
        }
    }

    private void ServerDie(ulong attackingPlayer)
    {
        Debug.Log($"[Server] Die on {name} by {attackingPlayer}");

        SpawnManager.instance.RespawnPlayerServerRpc(attackingPlayer);
        SpawnManager.instance.RespawnPlayerServerRpc(OwnerClientId);

        WinTracker.Singleton.AddWin(attackingPlayer);


         _isDead = false;
    }

}




