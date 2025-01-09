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
    [SerializeField] public GameObject ShowCasePrefab;





    public int Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject Prefab => WandPrefab;
}
