using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenButtons : NetworkBehaviour
{

    // Method to restart the game
    public void RestartGame()
    {
        if(IsServer)
        {
            //shouldnt run atm casue its called by button
            print("server made it to call Reset Win");
        }
        print("Reseting and movijng back to home screen");
        // Call the one-shot server RPC (works from client thanks to RequireOwnership = false)
        WinTracker.Singleton.ResetWinsAndReturnToSelectServerRpc();
    }
    //method to leave lobby and to go to main menu
    public void LeaveGame()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        if (ServerManager.Instance != null && ServerManager.Instance.Lobby != null) // <- fixed null check
        {
            ServerManager.Instance.LeaveLobby();
        }

        // After shutdown, use normal Unity scene load
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            gameController.GC.CharacterSelectSceneName,
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }
}



