using AssetInventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameController : MonoBehaviour
{

    public static gameController GC;
    public  PlayerController[] Players;
    [SerializeField] bool HideMouse;
    public string PLayerTag = "Player";
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

}



