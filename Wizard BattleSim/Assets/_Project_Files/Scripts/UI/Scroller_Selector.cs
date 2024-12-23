using Unity.Netcode;
using UnityEngine;

public class Scroller_Selector : NetworkBehaviour
{
    public WandDatabase wandDatabase;
    public CharacterDatabase characterDatabase;
    public SpellBook_AllSpellsList SpellDatabase;
    public enum SelectionType
    {
        Wand,
        Character,
        Spell
    }
    public SelectionType selectionType;
    public Scroller_InfoButton[] windows;
    public GameObject Confirmed_Icon;
    public GameObject[] InUseConfirmed_icons;
    public int CurrentWindow;
    public GameObject Selected_icon;
    public GameObject CurrentIcon;
    public GameObject current_Icon_Indicator;
    public int CurrentIconIndex;
    public int SpellIndex;
    public bool ReadyToStart;
    public GameObject Start_button;
    public void Start()
    {
        CurrentWindow = 0;
        CurrentIconIndex = 0;
        InUseConfirmed_icons = new GameObject[windows.Length];
        windows[CurrentWindow].gameObject.SetActive(true);
        selectionType = windows[CurrentWindow].type;
        windows[CurrentWindow].SetInfoPanel(CurrentIconIndex);
        if (Selected_icon != null)
        {
            CurrentIcon = Instantiate(current_Icon_Indicator, windows[CurrentWindow].transform);
        }
    }
    public void MoveToNextCat()
    {
        // Move to next category by enabling the next window
        if (CurrentWindow + 1 < windows.Length) // Prevent out-of-bounds
        {
            CurrentIconIndex = 0;
            windows[CurrentWindow].ConfirmButton.interactable = false;
            InUseConfirmed_icons[CurrentWindow] = Instantiate(Confirmed_Icon, windows[CurrentWindow].SelectorIconHolder.transform);
            windows[CurrentWindow].DisableButtons();
            windows[CurrentWindow].LockedIn = true;
            windows[CurrentWindow].SetInfoPanel(CurrentIconIndex);
            CurrentWindow++;
            windows[CurrentWindow].gameObject.SetActive(true);
            selectionType = windows[CurrentWindow].type;
            windows[CurrentWindow].SetInfoPanel(CurrentIconIndex);
            windows[CurrentWindow].EnableButtons();
            


            if (CurrentIcon != null)
            {
                Destroy(CurrentIcon);
                print("Destroying Current Icon");
            }
            if (current_Icon_Indicator != null)
            {
                Destroy(InUseConfirmed_icons[CurrentWindow]);
                CurrentIcon = Instantiate(current_Icon_Indicator, windows[CurrentWindow].SelectorIconHolder.transform);
            }
        }
        else
        {
            if(CurrentIcon)
            {
                Destroy(CurrentIcon);
                windows[CurrentWindow].DisableButtons();
                InUseConfirmed_icons[CurrentWindow] = Instantiate(Confirmed_Icon, windows[CurrentWindow].SelectorIconHolder.transform);
            }
            Debug.LogWarning("No more categories to move to.");
        }
        foreach (var window in windows)
        {
            if (window.LockedIn)
            {
                ReadyToStart = true;
                continue;
            }
            else
            {
                ReadyToStart = false;
                return;
            }
        }
        if (IsHost && ReadyToStart)
        {
            Start_button.SetActive(true);
        }
    }

    public void MoveToPreviousCat()
    {
        if (!IsClient)
        {
            return;
        }
        //Move to previous category by enabling the previous window and disabling any windows after it

        Start_button.SetActive(false);
        if (CurrentWindow == windows.Length - 1 && windows[CurrentWindow].LockedIn)
        {
            print("reopeing final cat");
            windows[CurrentWindow].EnableButtons();
            Destroy(InUseConfirmed_icons[CurrentWindow]);
            Destroy(CurrentIcon);
            windows[CurrentWindow].ConfirmButton.interactable = true;
            windows[CurrentWindow].SetInfoPanel(CurrentIconIndex);
            windows[CurrentWindow].LockedIn = false;
        }
        else if (CurrentWindow != 0)
        {
            if (CurrentIcon != null)
            {
                Destroy(CurrentIcon);
            }
            CurrentIconIndex = 0;
            InUseConfirmed_icons[CurrentWindow] = Instantiate(Confirmed_Icon, windows[CurrentWindow].SelectorIconHolder.transform);
            windows[CurrentWindow].DisableButtons();
            CurrentWindow--;
            windows[CurrentWindow].EnableButtons();
            Destroy(InUseConfirmed_icons[CurrentWindow]);
            windows[CurrentWindow].ConfirmButton.interactable = true;
            windows[CurrentWindow].SetInfoPanel(CurrentIconIndex);
        }


        if(current_Icon_Indicator != null)
        {
            CurrentIcon = Instantiate(current_Icon_Indicator, windows[CurrentWindow].SelectorIconHolder.transform);
        }

    }

    public void ScrollUp()
    {
        if(!IsClient)
        {
            return;
        }
        CurrentIconIndex++;

        switch (selectionType)
        {
            case SelectionType.Wand:
                //scroll through the wand window
                if(CurrentIconIndex == wandDatabase.GetAllWands().Length)
                {
                    CurrentIconIndex = 0;
                }
                break;
            case SelectionType.Spell:
                if(CurrentIconIndex == SpellDatabase.SpellBook.Count)
                {
                    CurrentIconIndex = 0;
                }
                //scroll through the spell window
                break;
            case SelectionType.Character:
                if(CurrentIconIndex == characterDatabase.GetAllCharacters().Length)
                {
                    CurrentIconIndex = 0;
                }
                //scroll through the character window
                break;
        }
        selectionType = windows[CurrentWindow].type;
        windows[CurrentWindow].SetInfoPanel(CurrentIconIndex);

        //scroll through the current window by setting the current windows stats to the next one in the list
    }

    public void ScrollDown()
    {
        if(!IsClient)
        {
            return;
        }
        if (CurrentIconIndex <= 0)
        {
            switch (selectionType)
            {
                case SelectionType.Wand:
                    CurrentIconIndex = wandDatabase.GetAllWands().Length -1;
                    break;
                case SelectionType.Spell:
                    CurrentIconIndex = SpellDatabase.SpellBook.Count - 1;
                    break;
                case SelectionType.Character:
                    CurrentIconIndex = characterDatabase.GetAllCharacters().Length - 1;
                    break;
            }
        }
        else
        {


            CurrentIconIndex--;
            switch (selectionType)
            {
                case SelectionType.Wand:
                    //scroll through the wand window
                    if (CurrentIconIndex == wandDatabase.GetAllWands().Length)
                    {
                        CurrentIconIndex = wandDatabase.GetAllWands().Length - 1;
                    }
                    break;
                case SelectionType.Spell:
                    if (CurrentIconIndex == SpellDatabase.SpellBook.Count)
                    {
                        CurrentIconIndex = SpellDatabase.SpellBook.Count - 1;
                    }
                    //scroll through the spell window
                    break;
                case SelectionType.Character:
                    if (CurrentIconIndex == characterDatabase.GetAllCharacters().Length)
                    {
                        CurrentIconIndex = characterDatabase.GetAllCharacters().Length - 1;
                    }
                    //scroll through the character window
                    break;
            }
            selectionType = windows[CurrentWindow].type;
            windows[CurrentWindow].SetInfoPanel(CurrentIconIndex);

            //scroll through the current window by setting the current windows stats to the previous one in the list
        }
    }

    public void ConfirmSelection()
    {
        if(!IsClient)
        {
            return;
        }
        windows[CurrentWindow].ConfirmButton.interactable = false;
        windows[CurrentWindow].LockedIn = true;
        print("Confirming selection");
        ConfirmSelectionServerRpc(selectionType, CurrentIconIndex, SpellIndex);
        // Move to the next category
        MoveToNextCat();

    }

    [ServerRpc(RequireOwnership = false)]
    public void ConfirmSelectionServerRpc(SelectionType selectionType,  int id, int SpellIndex, ServerRpcParams serverRpcParams = default)
    {
        print("Confirming selection Rpc");

        if (windows == null || CurrentWindow >= windows.Length)
        {
            Debug.LogError($"Invalid windows array or CurrentWindow out of bounds. CurrentWindow: {CurrentWindow}, windows.Length: {windows?.Length}");
            return;
        }
        switch (selectionType)
        {
            case SelectionType.Wand:
                PlayerStateManager.Singleton.AddState(
                    new CharacterSelectState(serverRpcParams.Receive.SenderClientId, wandID: id));
                break;
            case SelectionType.Spell:
                if (SpellIndex == 0)
                    PlayerStateManager.Singleton.AddState(
                        new CharacterSelectState(serverRpcParams.Receive.SenderClientId, spell0: id));
                else if (SpellIndex == 1)
                    PlayerStateManager.Singleton.AddState(
                        new CharacterSelectState(serverRpcParams.Receive.SenderClientId, spell1: id));
                else
                    PlayerStateManager.Singleton.AddState(
                        new CharacterSelectState(serverRpcParams.Receive.SenderClientId, spell2: id));
                break;
            case SelectionType.Character:
                PlayerStateManager.Singleton.AddState(
                    new CharacterSelectState(serverRpcParams.Receive.SenderClientId, characterId: id));
                break;
        }
    }


    public void CancelSelection()
    {
        //Unlock the current selection and allows for a new selection
        windows[CurrentWindow].LockedIn = false;
        windows[CurrentWindow].ConfirmButton.interactable = true;
        Destroy(InUseConfirmed_icons[CurrentWindow]);
        Destroy(CurrentIcon);
        CurrentIcon = Instantiate(current_Icon_Indicator, windows[CurrentWindow].SelectorIconHolder.transform);

    }

    




}
