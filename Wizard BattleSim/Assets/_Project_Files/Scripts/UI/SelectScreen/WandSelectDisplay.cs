using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WandSelectDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private WandDatabase wandDatabase;
    [SerializeField] private Transform wandsHolder;
    [SerializeField] private WandSelectButton wandButtonPrefab;
    [SerializeField] private CharacterSelectDisplay characterSelectDisplay;
    [SerializeField] private GameObject SelectionInfo_icons;
    private List<WandSelectButton> wandButtons = new List<WandSelectButton>();

    private void Start()
    {
        InitializeWandSelection();
    }



    private void InitializeWandSelection()
    {

        foreach (var wand in wandDatabase.GetAllWands())
        {
            var wandButtonInstance = Instantiate(wandButtonPrefab, wandsHolder);
            wandButtonInstance.SetWand(this, wand);
            wandButtons.Add(wandButtonInstance);
        }
    }

    public override void OnNetworkSpawn()
    {
        characterSelectDisplay.players.OnListChanged += HandlePlayersStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        
        characterSelectDisplay.players.OnListChanged -= HandlePlayersStateChanged;
        
    }

    public void SelectWand(Wand wand)
    {
        SelectWandServerRpc(wand.Id);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectWandServerRpc(int wandId, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        for (int i = 0; i < characterSelectDisplay.players.Count; i++)
        {
            if (characterSelectDisplay.players[i].ClientId != clientId) { continue; }

            characterSelectDisplay.players[i] = new CharacterSelectState(
                characterSelectDisplay.players[i].ClientId,
                characterSelectDisplay.players[i].CharacterId,
                wandId,
                default,
                default,
                default,

                characterSelectDisplay.players[i].IsLockedIn);




            Debug.Log($"Client {clientId} selected wand {wandId}");
            break;
        }
    }

    private void HandlePlayersStateChanged(NetworkListEvent<CharacterSelectState> changeEvent)
    {
        for (int i = 0; i < characterSelectDisplay.playerCards.Length; i++)
        {
            if (characterSelectDisplay.players.Count > i)
            {
                characterSelectDisplay.playerCards[i].UpdateDisplay(characterSelectDisplay.players[i]);
            }
            else
            {
                characterSelectDisplay.playerCards[i].DisableDisplay();
            }
        }

    }
}
