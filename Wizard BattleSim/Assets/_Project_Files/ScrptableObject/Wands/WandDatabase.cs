using UnityEngine;
using System.Linq;


[CreateAssetMenu(fileName = "WandDatabase", menuName = "Wands/WandDatabase")]

public class WandDatabase : ScriptableObject
{
    [SerializeField] public Wand[] Wands = new Wand[0];

    public Wand[] GetAllWands() => Wands;

    public Wand GetWandById(int id)
    {
        foreach (var Wand in Wands)
        {
            if (Wand.Id == id)
            {
                return Wand;
            }
        }

        return null;
    }

    public bool IsValidWandId(int id)
    {
        return Wands.Any(x => x.Id == id);
    }
}

