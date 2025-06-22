using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Spell))]
public class SpellEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target object (the current instance of the Spell ScriptableObject)
        Spell spell = (Spell)target;

        // Custom header with color
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.normal.textColor = Color.cyan;

        // General Info Section
        EditorGUILayout.LabelField("Spell General Info", headerStyle);
        spell.Spell_Name = EditorGUILayout.TextField(new GUIContent("Spell Name", "The name of the spell displayed to players."), spell.Spell_Name);
        spell.SpellIcon = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Spell Icon", "The icon representing the spell in the UI."), spell.SpellIcon, typeof(Sprite), allowSceneObjects: false);
        spell.ManaCost = EditorGUILayout.FloatField(new GUIContent("Mana Cost", "The mana cost required to cast this spell."), spell.ManaCost);
        spell.CooldownDuration = EditorGUILayout.FloatField(new GUIContent("Cooldown Duration", "The cooldown duration before the spell can be cast again (in seconds)."), spell.CooldownDuration);
        spell.Spell_CastTime = EditorGUILayout.FloatField(new GUIContent("Spell Cast Time", "The time it takes to cast the spell (in seconds)."), spell.Spell_CastTime);
        spell.CasterChar = (Character)EditorGUILayout.ObjectField(new GUIContent("Caster Character", "The Client id of who cast the spell"), spell.CasterChar, typeof(Character), allowSceneObjects: true);

        EditorGUILayout.Space();

        // Mechanics Section
        EditorGUILayout.LabelField("Spell Mechanics", headerStyle);
        spell.Spell_Speed = EditorGUILayout.FloatField(new GUIContent("Spell Speed", "The speed at which the spell travels."), spell.Spell_Speed);
        spell.Spell_Damage = EditorGUILayout.FloatField(new GUIContent("Spell Damage", "The damage dealt by the spell."), spell.Spell_Damage);
        spell.LifeTime = EditorGUILayout.FloatField(new GUIContent("Life Time", "The duration (in seconds) before the spell disappears."), spell.LifeTime);
        spell.MultiHitCooldown = EditorGUILayout.FloatField(new GUIContent("Multi Hit Cooldown", "The cooldown time (in seconds) between multi-hits for the spell."), spell.MultiHitCooldown);

        EditorGUILayout.Space();

        // Prefabs Section
        EditorGUILayout.LabelField("Spell Prefabs", headerStyle);
        spell.Spell_Prefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Spell Prefab", "The main prefab representing the spell."), spell.Spell_Prefab, typeof(GameObject), allowSceneObjects: false);
        spell.Air_SpellToSpawn_Prefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Air Spell Prefab", "The prefab for the air version of the spell."), spell.Air_SpellToSpawn_Prefab, typeof(GameObject), allowSceneObjects: false);
        spell.Grounded_SpellToSpawn_Prefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Grounded Spell Prefab", "The prefab for the grounded version of the spell."), spell.Grounded_SpellToSpawn_Prefab, typeof(GameObject), allowSceneObjects: false);
        spell.Spell_display_Prefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Display Prefab ", "The prefab for the Display version of the spell."), spell.Spell_display_Prefab, typeof(GameObject), allowSceneObjects: false);

        EditorGUILayout.Space();

        // Spawnable Effects Section
        EditorGUILayout.LabelField("Spawnable Effects", headerStyle);
        spell.ImpactEffect = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Impact Effect", "The visual effect triggered when the spell impacts something."), spell.ImpactEffect, typeof(GameObject), allowSceneObjects: false);

        EditorGUILayout.Space();

        // Sounds Section
        EditorGUILayout.LabelField("Sounds", headerStyle);
        spell.FireSound = (AudioClip)EditorGUILayout.ObjectField(new GUIContent("Fire Sound", "The sound effect played when the spell is fired."), spell.FireSound, typeof(AudioClip), allowSceneObjects: false);
        spell.ImpactSound = (AudioClip)EditorGUILayout.ObjectField(new GUIContent("Impact Sound", "The sound effect played when the spell impacts something."), spell.ImpactSound, typeof(AudioClip), allowSceneObjects: false);
        // Add the SoundVolume with a slider range from 0 to 1
        spell.SoundVolume = EditorGUILayout.Slider(new GUIContent("Sound Volume", "The Sound Volume"), spell.SoundVolume, 0f, 1f);

        // Add the SoundPriority with an integer slider range from 0 to 255
        spell.SoundPriority = EditorGUILayout.IntSlider(new GUIContent("Sound Priority", "The Sound Priority"), spell.SoundPriority, 0, 255);

        // Add the SoundPitch with an integer slider range from 1 to 3
        spell.SoundPitch = EditorGUILayout.IntSlider(new GUIContent("Sound Pitch", "The Sound Pitch"), spell.SoundPitch, -3, 3);

        EditorGUILayout.Space();

        // Spell Type Section
        EditorGUILayout.LabelField("Type of Spell", headerStyle);
        spell.Spell_Type = (Spell.SpellType)EditorGUILayout.EnumPopup(new GUIContent("Spell Type", "The type of the spell: Light, Mid, or Heavy."), spell.Spell_Type);

        // Display the spell icon (if it exists) in the Inspector
        if (spell.SpellIcon != null)
        {
            GUILayout.Label("Spell Icon:");
            GUILayout.Label(spell.SpellIcon.texture, GUILayout.Width(64), GUILayout.Height(64));
        }

        // Apply changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(spell);
        }
    }

    // This method allows us to display the custom icon in the Project Window
    [InitializeOnLoadMethod]
    static void Initialize()
    {
        // Hook into the Project Window GUI event to draw custom icons
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    // Draws the Spell icon in the Project View, showing both the icon and the name
    static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        // Get the asset path and load the corresponding ScriptableObject
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var spell = AssetDatabase.LoadAssetAtPath<Spell>(path);

        // If the asset is a Spell and has an icon, draw the custom icon
        if (spell != null && spell.SpellIcon != null)
        {
            // Create a Rect for the custom icon, aligned to the left
            Rect iconRect = new Rect(selectionRect.x, selectionRect.y - 5, 64, 64); // Smaller 16x16 icon on the left side

            // Draw the custom icon within this smaller Rect
            GUI.DrawTexture(iconRect, spell.SpellIcon.texture);

            // The name will be automatically displayed next to the icon by Unity
        }
    }
}
