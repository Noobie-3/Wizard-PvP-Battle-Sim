using AssetInventory;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
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
    private void Awake() {

        if(GC == null) {
            GC = this;

        }
        else {
            Destroy(this.gameObject);
        }


         


    }
    private void Start() {

        if (HideMouse)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

        }
        else {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

        }

    }



    private void Update() {
        //if PlayerList is less than the number of players in the scene)
        if(Players.Length != GameObject.FindGameObjectsWithTag(PLayerTag).Length) {
            Players = FindObjectsOfType<PlayerController>();
        }
    }


    public GameObject PlaySoundAtLocation(Transform Position, AudioClip Sound, float Volume = .5f, int Priority = 140, int Pitch = 1)
    {
        var SoundObject = Instantiate(PlaySoundPrefab, Position.position, Quaternion.identity);
        
        SoundObject.GetComponent<PlaySoundAtLocation>().sound = Sound;
        SoundObject.GetComponent<PlaySoundAtLocation>().SetvaluesAndPlay(Volume, Priority, Pitch);
        return SoundObject;
    }
}



