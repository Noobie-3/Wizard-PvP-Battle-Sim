using UnityEngine;

public class WinScreen_Load : MonoBehaviour
{
    public static WinScreen_Load Instance;
    [SerializeField] private Transform resultContainer;  // Container for result rows
    [SerializeField] private GameObject resultPrefab;  // Prefab for displaying a result (name and score)
    [SerializeField] private Transform[] ModelsSpawnPoints;
    [SerializeField] private GameObject[] SpawnedModels;
    [SerializeField] private CharacterDatabase characterDatabase; // Reference to the character database

    private void Awake()
    {
        // Singleton pattern for easy access from anywhere
        Instance = this;
    }

    // Method called to display results when the game ends
    public void ShowResults(CharacterSelectState[] results)
    {
        // Clear any previous results
        foreach (Transform child in resultContainer)
        {
            Destroy(child.gameObject);
        }

        // Instantiate results for each player up to 3

        
        for (int i = 0; i < 2; i++)
        {
            print("the results length is: " + results.Length.ToString() + " i is: " + i.ToString() + " Player Name: " + results[i].PLayerDisplayName.ToString());
            if (results.Length <= i)
            {
                break;
            }
            print("Player Name: " + results[i].PLayerDisplayName.ToString() + " Player Rank: " + results[i].Ranking);
            GameObject obj = Instantiate(resultPrefab, resultContainer);
            obj.GetComponent<ResultsUI>().SetInfo(results[i].PLayerDisplayName.ToString(), results[i].WinCount, results[i].Ranking, characterDatabase.GetCharacterById(results[i].CharacterId).Icon)
                ;
            // Spawn the character model at the corresponding spawn point on the podium
            
            if(SpawnedModels[i] == null)
            {
                SpawnedModels[i] = Instantiate(characterDatabase.GetCharacterById(results[i].CharacterId).resultsPreFab, ModelsSpawnPoints[i].position, ModelsSpawnPoints[i].rotation);
            }
        }
        
    }
}


