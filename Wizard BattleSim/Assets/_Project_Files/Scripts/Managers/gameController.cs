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


/*    public void SpawnPlayers(ulong ClientId)
    {
        var spawnLocations = FindObjectsOfType<PlayerSpawnLocation>();
        var AllPlayerControllers = FindObjectsOfType<PlayerController>();


        foreach (var PC in AllPlayerControllers)
        {

            if (PC.NetworkObject.OwnerClientId != ClientId)
            {
                continue;
            }


            for (int i = 0; i < spawnLocations.Length; ++i)
            {
                if (spawnLocations[i] == null)
                {
                    return;
                }
                if (spawnLocations[i].CanSpawnPlayer && PC.isSpawned == false)
                {
                    spawnLocations[i].SetCanSpawnClientRpc(false);
                    PC.MySpawn = spawnLocations[i];
                    PC.isSpawned = true;
                    if (PC.rb == null) return;
                    PC.rb.MovePosition(spawnLocations[i].transform.position);
                    print("the player " + PC.name + " has been spawned at " + spawnLocations[i].transform.position);
                    print(PC.transform.position);
                }


            }
        }

    }
*/
}



