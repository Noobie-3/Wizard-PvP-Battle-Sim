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

[RequireComponent(typeof(PlayerController))]
public class SpellCaster : NetworkBehaviour
{
    [SerializeField] public SpellBook_AllSpellsList SpellBook;
    [SerializeField] public GameObject CastTimeUi;
    [SerializeField] private TextMeshProUGUI CastSpellChargeText;
    [SerializeField] private float CastTimeProgress;
    [SerializeField] public UnityEngine.UI.Image CastTimeProgressUI;
    [SerializeField] public Coroutine CastTimeProgressEnum;
    [SerializeField] public Coroutine ChargeSpellIEnum;
    [SerializeField] private PlayerController Player;
    [SerializeField] int[] CurrentSpells = {0,1,2};
    [SerializeField] float[] CurrentSpellsTimers = {0,0,0};
    [SerializeField] public  int SelectedSpell;
    [SerializeField] public int MaxSpells;
    [SerializeField] public bool IsChangingSpell;
    [SerializeField] public bool IsCasting;



    private void FixedUpdate()
    {
        if (!IsOwner) return;
        HandleCoolDowns();
        isCastingSpell();
    }
    // Spell selection logic
    public void ScrollSpellSelection(float scrollInput)
    {
        if (!IsOwner) return;

        if (scrollInput > 0)
        {
            SelectedSpell--;
            if (SelectedSpell < 0)
                SelectedSpell = CurrentSpells[MaxSpells - 1];
        }
        else if (scrollInput < 0)
        {
            SelectedSpell++;
            if (SelectedSpell >= MaxSpells)
            {
                SelectedSpell = 0;
            }
        }
        print("The selected spell is: " + SelectedSpell + " out of " + MaxSpells);
    }



    public void StartSpellCast()
    {
        if(!IsOwner) return;
        IsCasting = true;
    }
    public  void CastSpell()
    { 
        if(!IsOwner) return;

            Player.Mana -= SpellBook.SpellBook[SelectedSpell].ManaCost;
        
        print("Casting spell");
        if (CastTimeProgress >= SpellBook.SpellBook[SelectedSpell].Spell_CastTime)
        {
            Vector3 ShotDir;
            ShotDir = Player.camComponent.transform.forward * 100;       
            print("the spell is being casted" + CurrentSpells[SelectedSpell] + " the client who fired the shots id is " + OwnerClientId);
            CastSpellServerRpc(CurrentSpells[SelectedSpell], transform.position, OwnerClientId, ShotDir);
            print("Spell casted");
            CurrentSpellsTimers[SelectedSpell] = SpellBook.SpellBook[SelectedSpell].CooldownDuration;
        }
    }
    public void isCastingSpell()
    {

        CastTimeUi.SetActive(true);
        if (IsCasting)
        { 
            CastTimeProgress += Time.deltaTime;
            var CastTimeProgressDecimal = CastTimeProgress / SpellBook.SpellBook[SelectedSpell].Spell_CastTime;
            CastTimeProgressUI.fillAmount = CastTimeProgressDecimal;
            CastSpellChargeText.text = (CastTimeProgressDecimal * 100).ToString() + "%";
            if(CastTimeProgress >= SpellBook.SpellBook[SelectedSpell].Spell_CastTime)
            {
                CastSpell();
                CastTimeProgress = 0;
                IsCasting = false;
            }

        }

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
    private void CastSpellServerRpc(int SpellToCast, Vector3 positon, ulong CasterId, Vector3 camDir)
    {
        print("Casting spell on server\n" + "Spell id is " + SpellToCast);
        GameObject CastedSpell =  Instantiate(SpellBook.SpellBook[SpellToCast].Spell_Prefab, positon, default);
        CastedSpell.GetComponent<NetworkObject>().Spawn();
        CastedSpell.GetComponent<ISpell_Interface>().Initialize(CasterId, camDir);
        CastedSpell.GetComponent<ISpell_Interface>().FireSpell();
    }
    public void SelectSpellWithKeyBoard(int spellIndex)
    {
        if (!IsOwner) return;
        TristanCast();
        if (spellIndex >= 0 && spellIndex < MaxSpells - 1)
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
        foreach (var spell in CurrentSpells)
        {
            if (CurrentSpellsTimers[spell] > 0)
            {
                CurrentSpellsTimers[spell] -= Time.deltaTime;
            }
        }
    }

}
