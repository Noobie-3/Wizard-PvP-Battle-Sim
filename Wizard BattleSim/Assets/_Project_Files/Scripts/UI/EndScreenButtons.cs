using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenButtons : MonoBehaviour
{

    // Method to restart the game
    public void RestartGame()
    {
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



