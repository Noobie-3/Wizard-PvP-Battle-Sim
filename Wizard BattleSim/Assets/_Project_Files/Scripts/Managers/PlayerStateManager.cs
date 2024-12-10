using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    [SerializeField] public List<CharacterSelectState> AllStatePlayers = new List<CharacterSelectState>();
    public static PlayerStateManager Singleton; private void Start()
    {
        //singleton 
        if (Singleton == null)
        {
            Singleton = this;
        }
        else if (Singleton != this)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(Singleton.gameObject);

    }

    //checks to see if a state with that client id already exist if not it adds it
    public void AddState(CharacterSelectState CSS)
    {
        // Check if a state with the same ClientId already exists
        int index = AllStatePlayers.FindIndex(state => state.ClientId == CSS.ClientId);

        if (index != -1)
        {
            // Get the existing state
            var existingState = AllStatePlayers[index];

            // Update only non-default values
            if (CSS.CharacterId != default) existingState.CharacterId = CSS.CharacterId;
            if (CSS.WandID != default) existingState.WandID = CSS.WandID;
            if (CSS.Spell0 != default) existingState.Spell0 = CSS.Spell0;
            if (CSS.Spell1 != default) existingState.Spell1 = CSS.Spell1;
            if (CSS.Spell2 != default) existingState.Spell2 = CSS.Spell2;

            existingState.IsLockedIn = CSS.IsLockedIn; // Always update boolean fields explicitly

            // Update the list with the merged state
            AllStatePlayers[index] = existingState;

            Debug.Log($"State for ClientId {CSS.ClientId} updated with new values.");
        }
        else
        {
            // Add new state if no match is found
            AllStatePlayers.Add(CSS);
            Debug.Log($"State for ClientId {CSS.ClientId} added.");
        }
    }
    //if state is in the array then it returns the one related to that client
    public CharacterSelectState LookupState(ulong ClientId = 0)
    {

        foreach (var state in AllStatePlayers)
        {
            if (state.ClientId == ClientId)
            {
                return state;
            }
            else
            {
                continue;
            }
        }
        return AllStatePlayers.FirstOrDefault();
        
    }
}
