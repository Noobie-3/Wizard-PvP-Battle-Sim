using AssetInventory;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class gameController : NetworkBehaviour
{

    public static gameController GC;
    public  PlayerController[] Players;
    [SerializeField] bool HideMouse;
    public float Gravity_force;
    public string PLayerTag = "Player";
    public LayerMask GroundLayer;
    public LayerMask WallLayer;
    public bool DebugMode;
    public string CharacterSelectSceneName;
    public string EndScreenSceneName;
    public GameObject PlaySoundPrefab;
    [SerializeField] private LobbyScreenSelector lobbyScreenSelector;
    public GameObject ConnectonTypeObject;

    public string HostIP;

    public enum ConnectionType     {
        Default,
        Online,
        Local
    }

    public ConnectionType connectionType = ConnectionType.Online;
    public TMP_Dropdown dropdown; // Assign this in the Inspector



    private void OnDropdownValueChanged(int index)
    {
        connectionType = (ConnectionType)index;
        print(connectionType + " selected.");

        switch(connectionType) {
            case ConnectionType.Online:
                lobbyScreenSelector.ChangeToJoin();
                ConnectonTypeObject.SetActive(false); // Hide dropdown after selection
                break;
            case ConnectionType.Local:
                lobbyScreenSelector.ChangeToNonOnlineLobby();
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetConnectionData(HostIP, 7777); // Direct IP
                ConnectonTypeObject.SetActive(false); // Hide dropdown after selection
                break;
        }

    }

    public void ResetConnectionType()
    {
       connectionType = ConnectionType.Default;
        dropdown.value = 0; // Reset dropdown to the first option
    }


    private void Awake() {

        if(GC == null) {
            GC = this;

        }
        else {
            Destroy(this.gameObject);
        }


         


    }
    private void Start() {
        DontDestroyOnLoad(this.gameObject);
        if (HideMouse)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

        }
        else {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

        }

        // Clear existing options
        dropdown.ClearOptions();

        // Add enum names to dropdown
        
        var options = ConnectionType.GetNames(typeof(ConnectionType));
        dropdown.AddOptions(new System.Collections.Generic.List<string>(options));

        // Optional: set default value
        dropdown.value = 0;
        foreach (var option in dropdown.options)
        {
            option.color = new Color(255, 0, 163, 255);
        }
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);


    }



    private void Update() {
        //if PlayerList is less than the number of players in the scene)
        if(Players.Length != GameObject.FindGameObjectsWithTag(PLayerTag).Length) {
            Players = FindObjectsOfType<PlayerController>();
        }
    }


    public GameObject PlaySoundAtLocation(Transform Position, AudioClip Sound, float Volume = .5f, int Priority = 140, int Pitch = 1, bool DoDestroy = true)
    {
        var SoundObject = Instantiate(PlaySoundPrefab, Position.position, Quaternion.identity);
        
        SoundObject.GetComponent<PlaySoundAtLocation>().sound = Sound;
        SoundObject.GetComponent<PlaySoundAtLocation>().SetvaluesAndPlay(Volume, Priority, Pitch, DoDestroy);
        return SoundObject;
    }

    public void DestroyObjectOnNetwork(GameObject ObjectToDestroy, int Time = 0)
    {
        Destroy(ObjectToDestroy, Time);
    }
}



