using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;  // Player's name text
    [SerializeField] private TextMeshProUGUI scoreText;  // Player's score text
    [SerializeField] private Image playerImage;  // Player's image (optional)
    [SerializeField] private Image rankImage;  // Player's rank image (optional)
    [SerializeField] private Sprite[] rankSprites;  // Array of rank sprites (0: 1st, 1: 2nd, etc.)

    // Method to set player's name and score
    public void SetInfo(string playerName, int score, int rank)
    {
        nameText.text = playerName;
        scoreText.text = "Wins: " + score;

        print("Player Name: " + playerName + "Player Rank: " + rank );
        switch(rank)
        {
            //ranking 1 through 10
            case 1:
                rankImage.sprite = rankSprites[0];  // 1st place
                break;
            case 2:
                rankImage.sprite = rankSprites[1];  // 2nd place
                break;
            case 3: 
                rankImage.sprite = rankSprites[2];  // 3rd place
                break;
            case 4:
                rankImage.sprite = rankSprites[3];  // 4th place
                break;
            case 5:
                rankImage.sprite = rankSprites[4];  // 5th place
                break;
            case 6:
                rankImage.sprite = rankSprites[5];  // 6th place
                break;
            case 7:
                rankImage.sprite = rankSprites[6];  // 7th place
                break;
            case 8:
                rankImage.sprite = rankSprites[7];  // 8th place
                break;
            case 9:
                rankImage.sprite = rankSprites[8];  // 9th place
                break;
            case 10:
                rankImage.sprite = rankSprites[9];  // 10th place
                break;
        }

    }
}
