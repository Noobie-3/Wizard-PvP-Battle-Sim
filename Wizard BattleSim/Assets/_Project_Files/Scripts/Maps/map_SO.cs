using UnityEngine;

[CreateAssetMenu(fileName = "New Map", menuName = "Map")]
public class Map_SO : ScriptableObject
{
    //Map Select Stats
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Display Name";
    [SerializeField] private Sprite icon;
    [SerializeField] private string mapName;
    //GamePlay Stats
    [SerializeField] public AudioClip LevelMusic;





    public int Id => id;
    public string DisplayName => displayName;
    public string MapName => mapName;
    public Sprite Icon => icon;


}
