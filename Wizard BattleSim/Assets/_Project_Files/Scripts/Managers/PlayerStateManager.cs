using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStateManager : NetworkBehaviour
{
    [SerializeField] public List<CharacterSelectState> AllStatePlayers = new List<CharacterSelectState>();
    public static PlayerStateManager Singleton; 
    private void Start()
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


    }

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(Singleton.gameObject);
        NetworkManager.OnClientConnectedCallback += OnPlayerSpawn;
        NetworkManager.OnClientDisconnectCallback -= OnPlayerDespawn;


        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            OnPlayerSpawn(client.ClientId);
        }
    }


    public void OnPlayerSpawn(ulong ClientID)
    {
        var TempState = new CharacterSelectState(ClientID);
        AddState(TempState);
    }

    public void OnPlayerDespawn(ulong ClientID) 
    {
        var tempstate = LookupState(ClientID);
        AllStatePlayers.Remove(tempstate);

    }

    //checks to see if a state with that client id already exist if not it adds it
    public void AddState(CharacterSelectState CSS)
    {

        var index = -1;
        for (int i = 0; i < AllStatePlayers.Count; i++)
        {
            if (AllStatePlayers[i].ClientId == CSS.ClientId)
            {
                index = i;
                break;
            }
            else
            {
                index = -1;
            }
        }
        if (index != -1)
        {


            // Get the existing state
            var existingState = AllStatePlayers[index];


            if (existingState.CharacterId != CSS.CharacterId && CSS.CharacterId != -1)
            {
                existingState.CharacterId = CSS.CharacterId;
            }
            if (existingState.WandID != CSS.WandID && CSS.WandID != -1)
            {
                existingState.WandID = CSS.WandID;
            }
            if (existingState.Spell0 != CSS.Spell0 && CSS.Spell0 != -1)
            {
                existingState.Spell0 = CSS.Spell0;
            }
            if (existingState.Spell1 != CSS.Spell1 && CSS.Spell1 != -1)
            {
                existingState.Spell1 = CSS.Spell1;
            }
            if (existingState.Spell2 != CSS.Spell1 && CSS.Spell2 != -1)
            {
                existingState.Spell2 = CSS.Spell2;
            }



            /*            // Update only non-default values amd values that are not the same as the existing state
                        if (    CSS.CharacterId != -1 || CSS.CharacterId != existingState.CharacterId) existingState.CharacterId = CSS.CharacterId;
                        if (CSS.WandID != -1 || CSS.WandID != existingState.WandID) existingState.WandID = CSS.WandID;
                        if (CSS.Spell0 != -1 || CSS.Spell0 != existingState.Spell0) existingState.Spell0 = CSS.Spell0;
                        if (CSS.Spell1 != -1 || CSS.Spell1 != existingState.Spell1) existingState.Spell1 = CSS.Spell1;
                        if (CSS.Spell2 != -1 || CSS.Spell2 != existingState.Spell2) existingState.Spell2 = CSS.Spell2;
            */
            existingState.IsLockedIn = CSS.IsLockedIn;

            // Update the list with the merged state
            AllStatePlayers[index] = existingState;

            Debug.Log($"State for ClientId {AllStatePlayers[index].ClientId} updated with new values.  Characterid: {AllStatePlayers[index].CharacterId}  WandId is: {AllStatePlayers[index].WandID}  Spell0: {AllStatePlayers[index].Spell0}  Spell1: {AllStatePlayers[index].Spell1}  Spell2: {AllStatePlayers[index].Spell2}");
        }

        else
        {
            // Add new state if no match is found
            AllStatePlayers.Add(CSS);
            Debug.Log($"State for ClientId {AllStatePlayers[index].ClientId} updated with new values.  Characterid: {AllStatePlayers[index].CharacterId}  WandId is: {AllStatePlayers[index].WandID}  Spell0: {AllStatePlayers[index].Spell0}  Spell1: {AllStatePlayers[index].Spell1}  Spell2: {AllStatePlayers[index].Spell2}");
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
