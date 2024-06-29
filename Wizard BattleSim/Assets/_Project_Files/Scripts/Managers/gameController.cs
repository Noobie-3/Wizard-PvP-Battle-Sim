using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameController : MonoBehaviour
{

    public static gameController GC;
    public  PlayerController Player;

    private void Awake() {

        if(GC == null) {
            GC = this;

        }
        else {
            Destroy(this.gameObject);
        }


         


    }
    private void Start() {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


    }
    private void Update() {
        if (Player == null) {
            Player = PlayerController.self;
        }
    }

}



