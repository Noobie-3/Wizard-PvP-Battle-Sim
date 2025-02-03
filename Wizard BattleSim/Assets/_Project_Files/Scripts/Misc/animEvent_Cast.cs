using UnityEngine;

public class animEvent_Cast : MonoBehaviour
{
    public SpellCaster SpellCaster;

    public void CastStart()
    {
        SpellCaster.StartSpellCast();
        print("SHould be frozen");

    }

    public void CastSpell()
    {
        SpellCaster.CastSpell();
        print ("Casting spell while frozen");
    }

    public void CastEnd()
    {
        SpellCaster.EndCast();
        print("Should be unfrozen");
    }
}
