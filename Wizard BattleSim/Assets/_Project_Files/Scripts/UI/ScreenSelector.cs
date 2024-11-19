using UnityEngine;
using UnityEngine.UI;

public class ScreenSelector : MonoBehaviour
{

    public GameObject[] Screens;
    public int currentScreen = 0;
    public Button NextButton;
    public Button PreviousButton;
    public CharacterSelectDisplay characterSelectDisplay;


    private void Start()
    {
        for (int i = 0; i < Screens.Length; i++) 
        {
            if(i == currentScreen) continue;
            Screens[i].SetActive(false);
        }
    }
    public void NextScreen()
    {

        if (currentScreen == Screens.Length -1) {
            characterSelectDisplay.StartGame();
            NextButton.gameObject.SetActive(false);
            return;
        }
        

        if (Screens[currentScreen].activeSelf)
        {
            Screens[currentScreen].SetActive(false);
        }
        if (currentScreen < Screens.Length - 1)
        {
            currentScreen++;
        }
        Screens[currentScreen].SetActive(true);
        
        PreviousButton.gameObject.SetActive(true);
    }

    public void PreviousScreen()
    {
        
        if (Screens[currentScreen].activeSelf)
        {
            Screens[currentScreen].SetActive(false);
        }
        if(currentScreen > 0)
        {            
            currentScreen--;
            if (currentScreen == 0)
            {
                PreviousButton.gameObject.SetActive(false);
            }
            }
            Screens[currentScreen].SetActive(true);
    }
}
