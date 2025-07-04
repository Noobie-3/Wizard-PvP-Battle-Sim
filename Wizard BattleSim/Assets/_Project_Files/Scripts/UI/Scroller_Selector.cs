using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class Scroller_Selector : NetworkBehaviour
{
    public WandDatabase wandDatabase; // Holds data for all available wands
    public CharacterDatabase characterDatabase; // Holds data for all available characters
    public SpellBook_AllSpellsList SpellDatabase; // Holds data for all available spells

    // Enum to define the type of selection
    public enum SelectionType
    {
        Wand,
        Character,
        Spell
    }
    public GameObject ObjectSpawned;// The object that is spawned such as a character, wand, or spell
    public Transform ObjectSpawn; // Spawn point for characters, Wands, and spells
    public float ObjectRespawnTime; // Time to wait before respawning the object
    public SelectionType selectionType; // Current type of selection (wand, character, or spell)
    public Scroller_InfoButton[] windows; // Array of window objects for categories
    public GameObject Confirmed_Icon; // Icon to display when selection is confirmed
    public GameObject[] InUseConfirmed_icons; // Array of confirmed icons for each window
    public int CurrentWindow; // Index of the currently active window
    public GameObject Selected_icon; // The selected icon object
    public GameObject CurrentIcon; // The current icon in use
    public GameObject current_Icon_Indicator; // Indicator for the currently selected icon
    public int CurrentIconIndex; // Index of the currently selected icon within the current window
    public int SpellIndex; // Index for spell selection
    public bool ReadyToStart; // Flag indicating if all selections are locked in
    public GameObject Start_button; // Button to start the game
    public AudioClip ClickSound; // Sound to play on button clicks
    public LookAtObjectCOnstant lookAtObjectCOnstant; // Script to make the camera look at a target
    public MusicManager MusicManager; // Script to manage music playback
    public GameObject Display;
    public Button StartButton; // Button to start the game, assigned in the inspector
    public LobbyScreenSelector ScreenSelector;
    public GameObject ConnectonTypeObject; // Object to show connection type, assigned in the inspector



    public void Start()
    {
        // Initialize variables and set up the first window
        CurrentWindow = 0;
        CurrentIconIndex = 0;
        InUseConfirmed_icons = new GameObject[windows.Length];
        windows[CurrentWindow].gameObject.SetActive(true); // Activate the first window
        selectionType = windows[CurrentWindow].type; // Set the selection type based on the first window
        windows[CurrentWindow].SetInfoPanel(CurrentIconIndex); // Set the info panel for the first icon

        // If a selected icon exists, create its indicator
        if (Selected_icon != null)
        {
            CurrentIcon = Instantiate(current_Icon_Indicator, windows[CurrentWindow].transform);
        }
        if(IsClient) return; // Ensure this code runs only on the client side
        ConnectonTypeObject.SetActive(true); 

    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.SceneManager.OnSceneEvent += OnSceneLoaded; // Subscribe to scene load events
    }
    private void OnSceneLoaded(SceneEvent Event)
    {
        if (Event.SceneEventType == SceneEventType.LoadEventCompleted && Event.SceneName == gameController.GC.CharacterSelectSceneName)
        {
            if (!IsClient) return;
            
            if (gameController.GC.connectionType == gameController.ConnectionType.Online)
            {
                ScreenSelector.ChangeToNonOnlineLobby();
                ConnectonTypeObject.SetActive(true); // Show connection type object if online
            }

        }
    }
    


    private void FixedUpdate()
    {
        if(Display.activeSelf && StartButton != null)
        {
            StartButton.onClick.AddListener(ServerManager.Instance.StartGame);
        }

        ObjectRespawnTime -= Time.deltaTime;
        if (ObjectRespawnTime <= 0)
        {
            if (selectionType == SelectionType.Character && ObjectSpawned != null) return;
            if (selectionType == SelectionType.Wand) return;
            if (!Display.activeSelf) return;
            Destroy(ObjectSpawned); // Destroy the previous object
            ReplaceSpawnedPrefab(); // Replace the spawned object with a new one


        }


        if(!Display.activeSelf)
        {
            if(ObjectSpawned != null) Destroy(ObjectSpawned); // Destroy the object if display is not active
        }



    }

    

    public void MoveToNextCat()
    {
        // Move to the next category by enabling the next window
        if (CurrentWindow + 1 < windows.Length) // Check if there is a next window
        {
            windows[CurrentWindow].StopIdleAnim(); // Stop playing idle animation for the current window
            CurrentIconIndex = 0; // Reset the icon index
            windows[CurrentWindow].ConfirmButton.interactable = false; // Disable the confirm button
            InUseConfirmed_icons[CurrentWindow] = Instantiate(Confirmed_Icon, windows[CurrentWindow].SelectorIconHolder.transform); // Add confirmed icon
            windows[CurrentWindow].DisableButtons(); // Disable buttons in the current window
            windows[CurrentWindow].LockedIn = true; // Lock the current selection
            CurrentWindow++; // Move to the next window
            windows[CurrentWindow].PlayIdleAnim(); // Play idle animation for the new window
            windows[CurrentWindow].gameObject.SetActive(true); // Activate the next window
            selectionType = windows[CurrentWindow].type; // Update the selection type
            windows[CurrentWindow].SetInfoPanel(CurrentIconIndex); // Update the info panel for the new window
            windows[CurrentWindow].EnableButtons(); // Enable buttons in the new window
            // Handle the current icon's indicator
            if (CurrentIcon != null)
            {
                Destroy(CurrentIcon); // Destroy the previous icon indicator
                print("Destroying Current Icon");
            }

            // Instantiate the new icon indicator
            if (current_Icon_Indicator != null)
            {
                Destroy(InUseConfirmed_icons[CurrentWindow]); // Remove the confirmed icon if any
                CurrentIcon = Instantiate(current_Icon_Indicator, windows[CurrentWindow].SelectorIconHolder.transform);
            }

            ReplaceSpawnedPrefab();

        }
        else
        {
            // Handle case where there are no more windows
            if (CurrentIcon)
            {
                Destroy(CurrentIcon);
                windows[CurrentWindow].DisableButtons();
                InUseConfirmed_icons[CurrentWindow] = Instantiate(Confirmed_Icon, windows[CurrentWindow].SelectorIconHolder.transform);
                windows[CurrentWindow].StopIdleAnim();
                Destroy(ObjectSpawned); // Destroy the currently spawned object
            }
            Debug.LogWarning("No more categories to move to.");
        }

        // Check if all selections are locked in
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

        // Activate the start button if ready and the current instance is the host
        if (IsHost && ReadyToStart)
        {
            Start_button.SetActive(true);
        }
    }

    public void MoveToPreviousCat()
    {
        if (!IsClient)
        {
            Debug.LogWarning("MoveToPreviousCat called on non-client instance.");
            return;
        }

        // Move to the previous category and update the UI
        Start_button.SetActive(false); // Hide the start button

        if (CurrentWindow == windows.Length - 1 && windows[CurrentWindow].LockedIn)
        {
            windows[CurrentWindow].PlayIdleAnim(); // Play idle animation for the final category
            // Handle re-opening the final category
            print("Reopening final category");
            windows[CurrentWindow].EnableButtons(); // Enable buttons in the current window
            Destroy(InUseConfirmed_icons[CurrentWindow]); // Remove confirmed icon
            Destroy(CurrentIcon); // Destroy current icon indicator
            windows[CurrentWindow].ConfirmButton.interactable = true; // Enable confirm button
            windows[CurrentWindow].SetInfoPanel(CurrentIconIndex); // Update the info panel
            windows[CurrentWindow].LockedIn = false; // Unlock the current window

        }
        else if (CurrentWindow > 0)
        {
            // Handle moving to a previous window
            if (CurrentIcon != null)
            {
                Destroy(CurrentIcon);
            }
            CurrentIconIndex = 0; // Reset the icon index
            InUseConfirmed_icons[CurrentWindow] = Instantiate(Confirmed_Icon, windows[CurrentWindow].SelectorIconHolder.transform); // Add confirmed icon
            windows[CurrentWindow].DisableButtons(); // Disable buttons in the current window
            windows[CurrentWindow].StopIdleAnim(); // stop playing idle animation for the current window
            windows[CurrentWindow].gameObject.SetActive(false); // Activate the previous window
            CurrentWindow--; // Move to the previous window
            windows[CurrentWindow].PlayIdleAnim(); // Play idle animation for the previous window
            windows[CurrentWindow].EnableButtons(); // Enable buttons in the previous window
            Destroy(InUseConfirmed_icons[CurrentWindow]); // Remove confirmed icon from the previous window
            windows[CurrentWindow].ConfirmButton.interactable = true; // Enable confirm button in the previous window
            windows[CurrentWindow].SetInfoPanel(CurrentIconIndex); // Update the info panel
            windows[CurrentWindow].LockedIn = false; // Unlock the previous window
        }
        selectionType = windows[CurrentWindow].type; // Update the selection type
        ReplaceSpawnedPrefab();
        // Instantiate the icon indicator for the new window
        if (current_Icon_Indicator != null)
        {
            CurrentIcon = Instantiate(current_Icon_Indicator, windows[CurrentWindow].SelectorIconHolder.transform);
        }
    }

    public void ScrollUp()
    {
        if (!IsClient)
        {
            return;
        }

        // Increment the icon index and wrap around if needed
        CurrentIconIndex++;
        switch (selectionType)
        {
            case SelectionType.Wand:
                if (CurrentIconIndex == wandDatabase.GetAllWands().Length)
                {
                    CurrentIconIndex = 0; // Wrap around to the first wand
                }
                break;
            case SelectionType.Spell:
                if (CurrentIconIndex == SpellDatabase.SpellBook.Count)
                {
                    CurrentIconIndex = 0; // Wrap around to the first spell
                }
                break;
            case SelectionType.Character:
                if (CurrentIconIndex == characterDatabase.GetAllCharacters().Length)
                {
                    CurrentIconIndex = 0; // Wrap around to the first character
                }
                break;
        }

        // Update the info panel with the new selection
        selectionType = windows[CurrentWindow].type;
        windows[CurrentWindow].SetInfoPanel(CurrentIconIndex);
        ReplaceSpawnedPrefab();
    }

    public void ScrollDown()
    {
        if (!IsClient)
        {
            return;
        }

        // Decrement the icon index and wrap around if needed
        if (CurrentIconIndex <= 0)
        {
            switch (selectionType)
            {
                case SelectionType.Wand:
                    CurrentIconIndex = wandDatabase.GetAllWands().Length - 1; // Wrap around to the last wand
                    break;
                case SelectionType.Spell:
                    CurrentIconIndex = SpellDatabase.SpellBook.Count - 1; // Wrap around to the last spell
                    break;
                case SelectionType.Character:
                    CurrentIconIndex = characterDatabase.GetAllCharacters().Length - 1; // Wrap around to the last character
                    break;
            }
        }
        else
        {
            CurrentIconIndex--; // Move to the previous index
        }

        // Update the info panel with the new selection
        selectionType = windows[CurrentWindow].type;
        windows[CurrentWindow].SetInfoPanel(CurrentIconIndex);
        ReplaceSpawnedPrefab();
    }

    public void ConfirmSelection()
    {




        // Confirm the current selection and lock the current window
        windows[CurrentWindow].ConfirmButton.interactable = false;
        windows[CurrentWindow].LockedIn = true;
        print("Confirming selection");

        //check if its the last window if so then lock in


        // Notify the server of the confirmed selection
        ConfirmSelectionServerRpc(selectionType, CurrentIconIndex, SpellIndex);

        // Move to the next category
        MoveToNextCat();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ConfirmSelectionServerRpc(SelectionType selectionType, int id, int SpellIndex, ServerRpcParams serverRpcParams = default)
    {
        print("Confirming selection Rpc");

        // Validate the window array and current window index
        if (windows == null || CurrentWindow >= windows.Length)
        {
            Debug.LogError($"Invalid windows array or CurrentWindow out of bounds. CurrentWindow: {CurrentWindow}, windows.Length: {windows?.Length}");
            return;
        }

        // Handle selection based on the type

        var State = PlayerStateManager.Singleton.LookupState(serverRpcParams.Receive.SenderClientId);
        switch (selectionType)
        {
            case SelectionType.Wand:
                PlayerStateManager.Singleton.AddState(new CharacterSelectState(serverRpcParams.Receive.SenderClientId, State.CharacterId, id, State.Spell0, State.Spell1, State.Spell2, State.IsLockedIn));
                break;
            case SelectionType.Spell:
                if (SpellIndex == 0)
                    PlayerStateManager.Singleton.AddState(new CharacterSelectState(serverRpcParams.Receive.SenderClientId,State.CharacterId, State.WandID, id, State.Spell1, State.Spell2, State.IsLockedIn));
                else if (SpellIndex == 1)
                    PlayerStateManager.Singleton.AddState(new CharacterSelectState(serverRpcParams.Receive.SenderClientId, State.CharacterId, State.WandID, State.Spell0, id, State.Spell2, State.IsLockedIn));
                else
                    PlayerStateManager.Singleton.AddState(new CharacterSelectState(serverRpcParams.Receive.SenderClientId, State.CharacterId, State.WandID, State.Spell0,State.Spell1, id, State.IsLockedIn));
                break;
            case SelectionType.Character:
                PlayerStateManager.Singleton.AddState(new CharacterSelectState(serverRpcParams.Receive.SenderClientId, id, State.WandID, State.Spell0, State.Spell1, State.Spell2, State.IsLockedIn));
                break;
        }

        if (CurrentWindow + 1 == windows.Length)
        {
            var state = PlayerStateManager.Singleton.LookupState(serverRpcParams.Receive.SenderClientId);
            PlayerStateManager.Singleton.AddState(new CharacterSelectState(serverRpcParams.Receive.SenderClientId, state.CharacterId, state.WandID, state.Spell0, state.Spell1, state.Spell2, true));
        }
    }



    public void ReplaceSpawnedPrefab()
    {
        print("Called Replace prefab");
        ObjectRespawnTime = 5;
        if (ObjectSpawned != null)
        {
            Destroy(ObjectSpawned);
        }


        switch (selectionType)
        {
            case SelectionType.Wand:
                ObjectSpawned = Instantiate(wandDatabase.GetWandById(CurrentIconIndex).ShowCasePrefab, ObjectSpawn);
                break;
            case SelectionType.Spell:
                ObjectSpawned = Instantiate(SpellDatabase.SpellBook[CurrentIconIndex].Spell_display_Prefab, ObjectSpawn);
                break;
            case SelectionType.Character:
                ObjectSpawned = Instantiate(characterDatabase.GetCharacterById(CurrentIconIndex).IntroPrefab, ObjectSpawn);
                MusicManager.PlaySong(characterDatabase.GetCharacterById(CurrentIconIndex).CharacterMusic);

                break;
        }
        lookAtObjectCOnstant.Target = ObjectSpawned.transform;

    }

    private void OnDisable()
    {
        if (ObjectSpawned != null)
        {
            Destroy(ObjectSpawned);
        }
        ResetAllSelections();
        

    }

    private void OnDestroy()
    {
        if (ObjectSpawned != null)
        {
            Destroy(ObjectSpawned); // Clean up the spawned object when the script is destroyed
        }
        NetworkManager.SceneManager.OnSceneEvent -= OnSceneLoaded; // Unsubscribe from scene load events
    }

    private void OnEnable()
    {
        windows[0].gameObject.SetActive(true);
        selectionType = windows[0].type;
        CurrentIconIndex = 0;
        windows[0].SetInfoPanel(CurrentIconIndex);

    }
    public void ResetAllSelections()
    {
        for (int i = CurrentWindow; i !> -1; i--)
        {
            MoveToPreviousCat();
        }
    }
}
