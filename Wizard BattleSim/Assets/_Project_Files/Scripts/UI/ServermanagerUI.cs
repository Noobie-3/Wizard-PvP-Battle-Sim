using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
public class ServermanagerUI : NetworkBehaviour
{
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    private NetworkVariable<int> PLayerNum = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    [SerializeField] private TextMeshProUGUI PLayerNumText;

    private void Awake()
    {// ADD THIS SCRIPT TO CANVSA OBJECT
        startServerButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });

        startClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });

        startHostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        
    }

    private void Update()
    {
        PLayerNumText.text = "Connected Player Count : " + PLayerNum.Value.ToString();


        if (!IsServer)
        {
            return;
        }

        PLayerNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }
}
