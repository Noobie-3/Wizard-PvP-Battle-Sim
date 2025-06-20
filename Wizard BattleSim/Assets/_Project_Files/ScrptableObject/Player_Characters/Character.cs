using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Characters/Character")]
public class Character : ScriptableObject
{
    //Character Select Stats
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Display Name";
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject introPrefab;
    [SerializeField] private GameObject gameplayPrefab;
    [SerializeField] private GameObject ResultsScreenPrefab;
    //GamePlay Stats
    [SerializeField] private float moveSpeed = 50;
    [SerializeField] int spamSpell;
    [SerializeField] public AudioClip CharacterMusic;





    public int Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject IntroPrefab => introPrefab;
    public GameObject GameplayPrefab => gameplayPrefab;
    public float MoveSpeed => moveSpeed;
    public int SpamSpell => spamSpell;
    public GameObject resultsPreFab => ResultsScreenPrefab;
}
