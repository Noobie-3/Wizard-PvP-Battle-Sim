using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinTracker : MonoBehaviour
{   
    public int winsNeeded;
    public Dictionary<ulong, int> PLayerWins = new Dictionary<ulong, int>();
    public static WinTracker Singleton;
    public int WinCount;
    public GameObject WinImage;
    public GameObject LoseImage;
    public Image[] PlayerImages;
    public CharacterDatabase characterDatabase;
    //Singleton Pattern
    private void Start()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }

        DontDestroyOnLoad(gameObject);

    }

    public  void AddWin(ulong ClientID)
    {
        if(WinCount >= PlayerImages.Length)
        {
            print("Win count is greater than player images length");
            return;

        }
        var PlayerState = PlayerStateManager.Singleton.LookupState(ClientID);
        if(PLayerWins.ContainsKey(ClientID))
        {
            PLayerWins[ClientID] = PLayerWins[ClientID] + 1;
        }
        else
        {
            PLayerWins.Add(ClientID, 1);
        }
        if(PlayerImages[WinCount] != null )
        {
            PlayerImages[WinCount].sprite = characterDatabase.GetCharacterById(PlayerState.CharacterId).Icon;
        }

        if(WinImage != null)
        {
            WinImage.gameObject.SetActive(true);
        }
        WinCount++;


    }

    public bool CheckWin(ulong ClientId)
    {
        bool result;

        if (PLayerWins[ClientId] >= winsNeeded) result = true;
        else result = false;


        foreach(var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (result)
            {
                // you can add placings later 
                if(player.ClientId == ClientId)
                {
                    if(WinImage != null)
                    {
                        Instantiate(WinImage, PlayerImages[0].transform.parent);
                    }
                }
                else
                {
                    if (LoseImage != null)
                    {
                        Instantiate(LoseImage, PlayerImages[0].transform.parent);
                    }
                }
            }
        }

        return result;
    }

    public void EndGame()
    {
        foreach(var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (player.PlayerObject != null) 
            {Destroy(player.PlayerObject);
            }                     

            //load end scene after enough wins
            if(gameController.GC.EndScreenSceneName != "")
            {
                SceneManager.LoadScene(gameController.GC.EndScreenSceneName);

            }
        }
        //logic to end the game and go to the win screen
    }
}
