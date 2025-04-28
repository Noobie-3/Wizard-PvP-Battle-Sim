using UnityEngine;

public class LobbyScreenSelector : MonoBehaviour
{
    [SerializeField ] public GameObject CreateLobbyScreen;
    [SerializeField ] public GameObject JoinLobbyScreen;
    [SerializeField ] public GameObject MainScreen;
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
        Debug.Log("Change to Join");
    }

    public void BackToMainScreen()
    {
        CreateLobbyScreen.SetActive(false);
        JoinLobbyScreen.SetActive(false);
        MainScreen.SetActive(true);
        Debug.Log("Back to Main Screen");
    }
}
