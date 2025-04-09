using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class SpellCaster : NetworkBehaviour
{
    [SerializeField] public SpellBook_AllSpellsList SpellBook;
    [SerializeField] public SpellBook_AllSpellsList SpellBook_Spammable;
    [SerializeField] public GameObject CastTimeUi;
    [SerializeField] private TextMeshProUGUI CastSpellChargeText;
    [SerializeField] public TextMeshProUGUI SpellName;
    [SerializeField] private float CastTimeProgress;
    [SerializeField] public UnityEngine.UI.Image CastTimeProgressUI;
    [SerializeField] public Coroutine CastTimeProgressEnum;
    [SerializeField] public Coroutine ChargeSpellIEnum;
    [SerializeField] private PlayerController Player;
    [SerializeField] private PlayerStats Stats; // Reference to PlayerStats

    [SerializeField] public int[] CurrentSpells = { 0, 1, 2 };
    [SerializeField] float[] CurrentSpellsTimers = { 0, 0, 0 };
    [SerializeField] public int SelectedSpell;
    [SerializeField] public int MaxSpells;
    [SerializeField] public bool IsChangingSpell;
    [SerializeField] public bool IsCasting;
    [SerializeField] public Transform CastPosition;
    [SerializeField] public Camera Cam;
    public List<float> spellCooldownTimers = new List<float>();
    public Transform Hand;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        Stats = GetComponent<PlayerStats>();  // Initialize the PlayerStats reference
        Player = GetComponent<PlayerController>();
        SetSpell(OwnerClientId);
    }

    private void FixedUpdate()
    {
        if(!IsOwner ) 
        {
            return;
        }
        HandleCoolDowns();
    }
    public void SetSpell(ulong id)
    {
        var State = PlayerStateManager.Singleton.LookupState(id);
        CurrentSpells[0] = State.Spell0;
        CurrentSpells[1] = State.Spell1;
        CurrentSpells[2] = State.Spell2;
        SetSpellServerRpc(id);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSpellServerRpc(ulong id)
    {
        var State = PlayerStateManager.Singleton.LookupState(id);
        CurrentSpells[0] = State.Spell0;
        CurrentSpells[1] = State.Spell1;
        CurrentSpells[2] = State.Spell2;
    }

    public void ScrollSpellSelection(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        var Scroll = context.ReadValue<float>();
        if (Scroll < 0)
        {
            SelectedSpell--;
            if (SelectedSpell < 0)
            {
                SelectedSpell = MaxSpells - 1;
            }
        }
        else if (Scroll > 0)
        {
            SelectedSpell++;
            if (SelectedSpell >= MaxSpells)
            {
                SelectedSpell = 0;
            }
        }

        Player.PlayerUi.UpdateUI();
    }

    public void StartSpellCast()
    {
        if (CurrentSpellsTimers[SelectedSpell] > 0)
        {
            Debug.Log("Spell on cooldown");
            return;
        }

        if (Stats.SpendMana(SpellBook.SpellBook[SelectedSpell].ManaCost))
        {
            if (Player.Anim == null)
            {
                return;
            }
            Player.Anim.SetBool("IsCasting", true);
            IsCasting = true;
            Player.CanRun = false;
            Player.MoveInput = Vector2.zero;


        }
    }

    [ConsoleCommand("Cast a spell")]
    public void CastSpell()
    {
        Vector3 ShotDir;
        RaycastHit hit;
        if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out hit, Mathf.Infinity))
        {
            ShotDir = hit.point;
        }
        else
        {
            ShotDir = Cam.transform.position + Cam.transform.forward * 100;
        }

        CastSpellServerRpc(CurrentSpells[SelectedSpell], CastPosition.position, OwnerClientId, ShotDir);
        CurrentSpellsTimers[SelectedSpell] = SpellBook.SpellBook[SelectedSpell].CooldownDuration;
        EndCast();
    }

    [ServerRpc()]
    private void CastSpellServerRpc(int SpellToCast, Vector3 positon, ulong CasterId, Vector3 camDir, bool Quicky = false)
    {
        GameObject CastedSpell;
        var manaCost = Quicky ? SpellBook_Spammable.SpellBook[SpellToCast].ManaCost
                              : SpellBook.SpellBook[SpellToCast].ManaCost;

        Stats.SpendMana(manaCost); // Deduct mana using the new system

        CastedSpell = Instantiate(SpellBook.SpellBook[SpellToCast].Spell_Prefab, positon, default);
        CastedSpell.GetComponent<NetworkObject>().Spawn();
        CastedSpell.GetComponent<ISpell_Interface>().Initialize(CasterId, camDir);
        CastedSpell.GetComponent<ISpell_Interface>().FireSpell();
    }

    public void EndCast()
    {
        if (!IsOwner) return;
        IsCasting = false;
        Player.CanRun = true;
        Player.MoveInput = Player.MoveAction.ReadValue<Vector2>();
        if (Player.Grounded)
        {
            Player.rb.linearVelocity = Vector3.zero;
        }

        Player.Anim.SetBool("IsCasting", false);
    }

    public void QuickCast()
    {
        if (!IsOwner) return;
        if (!Stats.SpendMana(SpellBook_Spammable.SpellBook[Stats.CharacterChosen.SpamSpell].ManaCost))
        {
            return;
        }

        Vector3 ShotDir;
        RaycastHit hit;
        if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out hit, Mathf.Infinity))
        {
            ShotDir = hit.point;
        }
        else
        {
            ShotDir = Cam.transform.position + Cam.transform.forward * 1000;
        }

        CastSpellServerRpc(Stats.CharacterChosen.SpamSpell, Hand.position, OwnerClientId, ShotDir, true);
    }

    public void TristanCast()
    {
        if (IsCasting)
        {
            StopCoroutine(ChargeSpellIEnum);
            IsCasting = false;
            Player.CanRun = true;
        }
    }
    public void SelectSpellWithKeyBoard(int spellIndex)
    {
        if (!IsOwner) return;
        TristanCast();
        if (spellIndex >= 0 && spellIndex < MaxSpells)
        {
            Debug.Log("Selected Spell: " + SelectedSpell + " out of " + MaxSpells);
        }
        else
        {
            Debug.LogWarning("Spell index out of range");
        }
    }

    void HandleCoolDowns()
    {
        for (int i = 0; i < CurrentSpells.Length; i++)
        {
            if (CurrentSpellsTimers[i] > 0)
            {
                CurrentSpellsTimers[i] -= Time.deltaTime;
            }
        }
    }
}
