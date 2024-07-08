using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    public static SpellManager Instance { get; private set; }

    public Dictionary<string, GameObject> spellPrefabs;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            spellPrefabs = new Dictionary<string, GameObject>();

            // Add your spell prefabs to the dictionary
            spellPrefabs.Add("Fireball", fireballPrefab);
            spellPrefabs.Add("IceSpike", iceSpikePrefab);
            // Add more spells as needed
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetSpellPrefabByName(string spellName)
    {
        if (spellPrefabs.ContainsKey(spellName))
        {
            return spellPrefabs[spellName];
        }
        return null;
    }

    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject iceSpikePrefab;
    // Add more spell prefabs as needed
}
