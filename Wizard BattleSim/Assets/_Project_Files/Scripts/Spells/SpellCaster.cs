using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.UI;
using System.Globalization;
using JetBrains.Annotations;
using Unity.VisualScripting;
using Unity.Services.Lobbies;

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
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        HandleCoolDowns();
        isCastingSpell();
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) return;

        print("Set the Spell for the player with the player id of " + OwnerClientId);
        SetSpell(OwnerClientId);
    }

    public void SetSpell(ulong id)
    {

        var State = PlayerStateManager.Singleton.LookupState(id);
        print("the spells that are being set are id: " + id + "spell0" + State.Spell0 + "Spell1" + State.Spell1 + "Spell2" + State.Spell2);
        CurrentSpells[0] = State.Spell0;
        CurrentSpells[1] = State.Spell1;
        CurrentSpells[2] = State.Spell2;
        SetSpellServerRpc(id);

    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSpellServerRpc(ulong id)
    {
        var State = PlayerStateManager.Singleton.LookupState(id);
        print("the spells that are being set are id: " + id + "spell0" + State.Spell0 + "Spell1" + State.Spell1 + "Spell2" + State.Spell2);
        CurrentSpells[0] = State.Spell0;
        CurrentSpells[1] = State.Spell1;
        CurrentSpells[2] = State.Spell2;
    }
    // Spell selection logic
    public void ScrollSpellSelection(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        //cycle through spells with mouse wheel
        var Scroll = context.ReadValue<float>();
        print("Scroll input " + Scroll);
        {

        }
        if (Scroll < 0)
        {
            SelectedSpell--;
            print(SelectedSpell + "what  Spell is selected ");
            if (SelectedSpell < 0)
            {
                SelectedSpell = MaxSpells - 1;
            }
        }
        else if (Scroll > 0)
        {
            SelectedSpell++;
            print(SelectedSpell + "what  Spell is selected ");
            if (SelectedSpell >= MaxSpells - 1)
            {
                SelectedSpell = 0;
            }
        }

        Player.PlayerUi.UpdateUI();
        print("Final Spell Selction affte scroll" + SelectedSpell);

    }



    public void StartSpellCast()
    {
        if (!IsOwner) return;
        IsCasting = true;
    }
    public void CastSpell()
    {
        if (!IsOwner) return;
        print("Casting spell");
        if (CastTimeProgress >= SpellBook.SpellBook[SelectedSpell].Spell_CastTime)
        {
            Player.moveSpeed = Player.moveSpeedDefault;
            Vector3 ShotDir;
            RaycastHit hit;
            if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out hit, Mathf.Infinity))
            {
                ShotDir = hit.point;
            }
            else
            {
                //default to 100 units in front of the player
                ShotDir = Cam.transform.position + Cam.transform.forward * 100;
            }
            CastSpellServerRpc(CurrentSpells[SelectedSpell], CastPosition.position, OwnerClientId, ShotDir);
            CurrentSpellsTimers[SelectedSpell] = SpellBook.SpellBook[SelectedSpell].CooldownDuration;
        }
    }
    public void isCastingSpell()
    {
        CastTimeUi.SetActive(true);
        if (IsCasting)
        {
            Player.moveSpeed = Player.moveSpeedDefault / 2;
            CastTimeProgress += Time.deltaTime;
            var CastTimeProgressDecimal = CastTimeProgress / SpellBook.SpellBook[SelectedSpell].Spell_CastTime;
            CastTimeProgressUI.fillAmount = CastTimeProgressDecimal;
            CastSpellChargeText.text = (CastTimeProgressDecimal * 100).ToString() + "%";
            SpellName.text = ("Casting spell: " + SpellBook.SpellBook[SelectedSpell].Spell_Name);
            
            if (CastTimeProgress >= SpellBook.SpellBook[SelectedSpell].Spell_CastTime)
            {

                CastSpell();
                CastTimeProgress = 0;
                IsCasting = false;
            }

        }

    }
    public void QuickCast()
    {if(!IsOwner) return;
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
        Debug.DrawLine(Cam.transform.position, ShotDir, Color.green, 2f); // Shows the full ray path


        CastSpellServerRpc(Player.CharacterChosen.SpamSpell, Hand.position, OwnerClientId, ShotDir, true);
    }


    //cancels Cast (name inspired by a dude named tristan)
    public void TristanCast()
    {
        if (IsCasting)
        {
            StopCoroutine(ChargeSpellIEnum);
            IsCasting = false;
        }
    }


    [ServerRpc()]
    private void CastSpellServerRpc(int SpellToCast, Vector3 positon, ulong CasterId, Vector3 camDir, bool Quicky = false)
    {
        GameObject CastedSpell;
        if (Quicky == true )
        {
            Player.Mana.Value -= SpellBook_Spammable.SpellBook[SpellToCast].ManaCost;
            CastedSpell = Instantiate(SpellBook_Spammable.SpellBook[SpellToCast].Spell_Prefab, positon, default);
        }
        else
        {
            print("Casting spell on server\n" + "Spell id" + SpellBook.SpellBook[SpellToCast].Spell_Name);
            Player.Mana.Value -= SpellBook.SpellBook[SelectedSpell].ManaCost;
            CastedSpell = Instantiate(SpellBook.SpellBook[SpellToCast].Spell_Prefab, positon, default);

        }

        CastedSpell.GetComponent<NetworkObject>().Spawn();
        CastedSpell.GetComponent<ISpell_Interface>().Initialize(CasterId, camDir);
        CastedSpell.GetComponent<ISpell_Interface>().FireSpell();
        
    }
    public void SelectSpellWithKeyBoard(int spellIndex)
    {
        if (!IsOwner) return;
        TristanCast();
        if (spellIndex >= 0 && spellIndex < MaxSpells )
        {
            Debug.Log("Selected Spell: " + SelectedSpell + " out of " + MaxSpells);
        }
        else
        {
            Debug.LogWarning("Spell index out of range");
        }
    }

    private void ChangeSpellSlot(int SpellSlot, int SpellToChangeTo)
    {
        IsChangingSpell = true;
        while(IsChangingSpell)
        {
            //handle Changing spell logic with buttons in ui here

        }
    }



    void HandleCoolDowns()
    {
        for (int i = 0; i< CurrentSpells.Length; i++)
        {
            if (CurrentSpellsTimers[i] > 0)
            {
                CurrentSpellsTimers[i] -= Time.deltaTime;
            }
        }
    }
    private void UpdateSpellCooldownTimers()
    {
     //   spellCooldownTimers = new List<float>(new float[currentSpells.Count]);
    }

    private void SpellCooldown()
    {
        for (int i = 0; i < spellCooldownTimers.Count; i++)
        {
            if (spellCooldownTimers[i] > 0)
            {
                spellCooldownTimers[i] -= Time.deltaTime;
            }
        }
    }

    /*  // Spell management
      public void AddSpell(Spell newSpell)
      {
          CurrentSpells.Add(newSpell);
          UpdateSpellCooldownTimers();
          PlayerUi.UpdateUI();
      }

      public void RemoveSpell(Spell spellToRemove)
      {
          int index = CurrentSpells.IndexOf(spellToRemove);
          if (index >= 0)
          {
              CurrentSpells.RemoveAt(index);
              UpdateSpellCooldownTimers();
              PlayerUi.UpdateUI();

          }
      }
  */

}
