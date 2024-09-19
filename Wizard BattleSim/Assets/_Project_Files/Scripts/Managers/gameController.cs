using AssetInventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameController : MonoBehaviour
{

    public static gameController GC;
    public  PlayerController[] Players;
    [SerializeField] bool HideMouse;
    public float Gravity_force;
    public string PLayerTag = "Player";
    public LayerMask GroundLayer;
    public LayerMask WallLayer;
    public bool DebugMode;
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

    public float TakeDmg(float Hp, float Def, float Attack, GameObject ObjectThatGotHit)
    {
        //take damage from the object that got hit while taking into account the defense of the object
        if(Def > Attack)
        {
            print(ObjectThatGotHit.name + " took no damage");
            //Effect for no damage here
            return Hp;
        }
        else {
            Hp -= (Attack - Def);
            //effect for damage here
        }

        return Hp;
    }

    private void Update() {
        //if PlayerList is less than the number of players in the scene)
        if(Players.Length != GameObject.FindGameObjectsWithTag(PLayerTag).Length) {
            Players = FindObjectsOfType<PlayerController>();
        }
    }

}



