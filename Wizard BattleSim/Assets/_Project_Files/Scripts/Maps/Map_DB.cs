using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Map_DB", menuName = "Scriptable Objects/Map_DB")]
public class Map_DB : ScriptableObject
{
    [SerializeField] public Map_SO[] Maps = new Map_SO[0];

    public Map_SO[] GetAllMaps() => Maps;

    public Map_SO GetMapById(int id)
    {
        foreach (var character in Maps)
        {
            if (id == -1)
            {
                return Maps[0];
            }
            else if (character.Id == id)
            {
                return character;
            }

        }

        return Maps[0];
    }

    public bool IsValidCharacterId(int id)
    {
        return Maps.Any(x => x.Id == id);
    }

}
