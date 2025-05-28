using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenButtons : MonoBehaviour
{

    // Method to restart the game
    public void RestartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(gameController.GC.CharacterSelectSceneName,loadSceneMode:LoadSceneMode.Additive);
    }

    //method to leave lobby and to go to main menu
    public void LeaveGame()
    {
        ServerManager.Instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        NetworkManager.Singleton.SceneManager.LoadScene(gameController.GC.CharacterSelectSceneName, loadSceneMode: LoadSceneMode.Single);
    }

}



