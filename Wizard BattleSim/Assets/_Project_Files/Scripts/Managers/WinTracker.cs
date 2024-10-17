using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WinTracker : MonoBehaviour
{   
    public int winsNeeded;
    public Dictionary<ulong, int> PLayerWins = new Dictionary<ulong, int>();
    public static WinTracker SingleTon;

    //SingleTon Pattern
    private void Start()
    {
        if(SingleTon == null)
        {
            SingleTon = this;
        }
        else if(SingleTon != this)
        {
            Destroy(gameObject);
        }
    }

    public  void AddWin(ulong ClientID)
    {
        if(PLayerWins.ContainsKey(ClientID))
        {
            PLayerWins[ClientID] = PLayerWins[ClientID] + 1;
        }
        else
        {
            PLayerWins.Add(ClientID, 1);
        }
    }

    public bool CheckWin(ulong ClientId)
    {
        bool result;

        if (PLayerWins[ClientId] >= winsNeeded) result = true;
        else result = false;

        return result;
    }
}
