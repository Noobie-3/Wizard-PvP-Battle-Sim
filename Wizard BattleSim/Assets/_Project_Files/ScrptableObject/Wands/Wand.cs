using UnityEngine;
using Unity.Netcode;
[CreateAssetMenu(fileName = "Wand", menuName = "Wands/Wand")]

public class Wand : ScriptableObject
{
    //Character Select Stats
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Wand";
    [SerializeField] private Sprite icon;
    [SerializeField] public GameObject WandPrefab;

    //GamePlay Stats
    [SerializeField] private float moveSpeed = 50;





    public int Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject Prefab => WandPrefab;
    public float MoveSpeed => moveSpeed;
}