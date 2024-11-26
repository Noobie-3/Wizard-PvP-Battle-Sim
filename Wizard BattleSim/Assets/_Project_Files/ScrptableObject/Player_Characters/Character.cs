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
    [SerializeField] private NetworkObject gameplayPrefab;

    //GamePlay Stats
    [SerializeField] private float moveSpeed = 50;
    [SerializeField] int spamSpell;





    public int Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject IntroPrefab => introPrefab;
    public NetworkObject GameplayPrefab => gameplayPrefab;
    public float MoveSpeed => moveSpeed;
    public int SpamSpell => spamSpell;
}
