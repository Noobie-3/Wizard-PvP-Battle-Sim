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


    public  void CastSpell()
    { if (!IsOwner) return;
        //RayCastTo Set Shot Direction
        UpdateUIForCasting();
        float CurrentCastTime = 0;
        while(CurrentCastTime < SpellBook.SpellBook[SelectedSpell].Spell_CastTime) { }
        {
            CurrentCastTime += Time.deltaTime;
        }
        RaycastHit hit;
        Vector3 ShotDir;
        if (Physics.Raycast(Player.camComponent.transform.position, Player.camComponent.transform.forward, out hit, Mathf.Infinity))
        {
            ShotDir = hit.point;
        }
        else
        {
            ShotDir = Player.camComponent.transform.forward;
        }
        print("the spell is being casted" + CurrentSpells[SelectedSpell]);
        CastSpellServerRpc(CurrentSpells[SelectedSpell], transform.position, NetworkObjectId, ShotDir );
        CurrentSpellsTimers[SelectedSpell] = SpellBook.SpellBook[SelectedSpell].CooldownDuration;
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
        CastedSpell.GetComponent<ISpell_Interface>().Initialize(CasterId, camDir);
        CastedSpell.GetComponent<NetworkObject>().Spawn();
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

    void UpdateUIForCasting()
    {
        CastTimeUi.SetActive(true);
        while(CastTimeProgress < 1 && IsCasting)
        {
            CastTimeProgress += Time.deltaTime / SpellBook.SpellBook[SelectedSpell].Spell_CastTime;
            CastTimeProgressUI.fillAmount = CastTimeProgress;
            CastSpellChargeText.text = (CastTimeProgress * 100).ToString() + "%";
        
        }
        print("Finished UI for casting ");
        CastTimeProgress = 0;
        CastTimeUi.SetActive(false);

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
