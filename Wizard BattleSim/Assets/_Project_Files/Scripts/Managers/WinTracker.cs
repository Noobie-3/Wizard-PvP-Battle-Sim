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
    public Image WinImage;
    public Image[] PlayerImages;
    //Singleton Pattern
    private void Start()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }

        DontDestroyOnLoad(gameObject);

    }

    public  void AddWin(ulong ClientID, Character characterWhoWon)
    {
        if(PLayerWins.ContainsKey(ClientID))
        {
            PLayerWins[ClientID] = PLayerWins[ClientID] + 1;
        }
        else
        {
            PLayerWins.Add(ClientID, 1);
        }
        if(PlayerImages[WinCount] != null)
        {
            PlayerImages[WinCount].sprite = characterWhoWon.Icon;
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
            SceneManager.LoadScene(gameController.GC.EndScreenSceneName);
        }
        //logic to end the game and go to the win screen
    }
}
