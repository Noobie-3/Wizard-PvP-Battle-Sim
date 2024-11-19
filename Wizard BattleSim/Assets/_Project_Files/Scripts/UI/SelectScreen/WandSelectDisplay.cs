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
                characterSelectDisplay.players[i].IsLockedIn
            );

            Debug.Log($"Client {clientId} selected wand {wandId}");
            break;
        }
    }
}
