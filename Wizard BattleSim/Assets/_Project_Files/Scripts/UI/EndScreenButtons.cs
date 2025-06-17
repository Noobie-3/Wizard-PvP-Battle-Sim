using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenButtons : MonoBehaviour
{

    // Method to restart the game
    public void RestartGame()
    {
        //reset values 
        //Reset spawn points list 
        SpawnManager.instance.spawnPoints = new PlayerSpawnLocation[0];
// Reset player spawn points dictionary 
        SpawnManager.instance.playerSpawnPoints.Clear();

        // Reset the player states
        WinTracker.Singleton.ResetWinsServerRpc();
        print("Restarting game and resetting spawn points.");
        // Load the character select scene
        NetworkManager.Singleton.SceneManager.LoadScene(gameController.GC.CharacterSelectSceneName,loadSceneMode:LoadSceneMode.Single);
    }

    //method to leave lobby and to go to main menu
    public void LeaveGame()
    {
        if(NetworkManager.Singleton.IsConnectedClient)
        {
            // If the player is Connected then ShutDown the NetworkManager
            NetworkManager.Singleton.Shutdown();
        }
        if (ServerManager.Instance.Lobby == null)
        {
            ServerManager.Instance.LeaveLobby();
        }
        NetworkManager.Singleton.SceneManager.LoadScene(gameController.GC.CharacterSelectSceneName, loadSceneMode: LoadSceneMode.Single);
    }

}



