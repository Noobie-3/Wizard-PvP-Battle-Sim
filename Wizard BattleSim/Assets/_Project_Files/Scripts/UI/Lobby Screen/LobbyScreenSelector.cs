using UnityEngine;

public class LobbyScreenSelector : MonoBehaviour
{
    [SerializeField ] public GameObject CreateLobbyScreen;
    [SerializeField ] public GameObject JoinLobbyScreen;
    [SerializeField ] public GameObject MainScreen;
    [SerializeField ] public GameObject NonOnlineLobbyScreen;

    public void ChangeToCreate()
    {
        JoinLobbyScreen.SetActive(false);
        MainScreen.SetActive(false);
        CreateLobbyScreen.SetActive(true);
        Debug.Log("Change to Create");
    }

    public void ChangeToJoin()
    {
        CreateLobbyScreen.SetActive(false);
        MainScreen.SetActive(false);
        JoinLobbyScreen.SetActive(true);
        NonOnlineLobbyScreen.SetActive(false);
        Debug.Log("Change to Join");
    }

    public void BackToMainScreen()
    {
        CreateLobbyScreen.SetActive(false);
        JoinLobbyScreen.SetActive(false);
        MainScreen.SetActive(true);
        NonOnlineLobbyScreen.SetActive(false);
        Debug.Log("Back to Main Screen");
    }

    public void ChangeToNonOnlineLobby()
    {
        NonOnlineLobbyScreen.SetActive(true);
        CreateLobbyScreen.SetActive(false);
        JoinLobbyScreen.SetActive(false);
        MainScreen.SetActive(false);
    }

    public void DisableAll()
    {
       CreateLobbyScreen.SetActive(false);
        JoinLobbyScreen.SetActive(false);
        MainScreen.SetActive(false);
        NonOnlineLobbyScreen.SetActive(false);
        gameController.GC.ConnectonTypeObject.SetActive(true);
        gameController.GC.ResetConnectionType();
        Debug.Log("Disable All Screens");
    }
}
