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
    public testRelay testRelay;
    public string joinCode;
    public TMP_InputField InputWindowInput;
    public TextMeshProUGUI joinCodeErrorText;
    private void Awake()
    {// ADD THIS SCRIPT TO CANVSA OBJECT
        startServerButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();

            startClientButton.gameObject.SetActive(false);
            startHostButton.gameObject.SetActive(false);
            startServerButton.gameObject.SetActive(false);
            PLayerNumText.gameObject.SetActive(false);
        });

        startClientButton.onClick.AddListener(() =>
        {
            //NetworkManager.Singleton.StartClient();
            //take in the input from user using input field
            if (joinCode != "")
            {


                testRelay.JoinRelay(joinCode);

                startClientButton.gameObject.SetActive(false);
                startHostButton.gameObject.SetActive(false);
                startServerButton.gameObject.SetActive(false);
                PLayerNumText.gameObject.SetActive(false);
            }
            else
            {
                joinCodeErrorText.text = "Please enter a join code first";
            }
        });

        startHostButton.onClick.AddListener(() =>
        {
           // NetworkManager.Singleton.StartHost();
           testRelay.CreateRelay();
            startClientButton.gameObject.SetActive(false);
            startHostButton.gameObject.SetActive(false);
            startServerButton.gameObject.SetActive(false);
            PLayerNumText.gameObject.SetActive(false);

        });

        InputWindowInput.onEndEdit.AddListener((string s) =>
        {
            joinCode = s;
        });
        
    }

    

    private void Update()
    {
        PLayerNumText.text = "Connected Player Count : " + PLayerNum.Value.ToString();


/*        if (IsServer)
        {
            PLayerNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
        }*/

    }
}
