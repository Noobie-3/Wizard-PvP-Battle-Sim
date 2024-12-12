using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpellBook[All Spells List]", menuName = "SpellBook[All Spells List]")]
public class SpellBook_AllSpellsList : ScriptableObject
{
    public List<Spell> SpellBook;
    

    public Spell LookUpSpell(int id) {

        if(id == -1) return SpellBook[0];
        else
        {
            return SpellBook[id];
        }

        
    }

}
