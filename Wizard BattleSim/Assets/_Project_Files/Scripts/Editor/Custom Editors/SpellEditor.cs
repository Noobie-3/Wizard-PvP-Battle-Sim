using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Spell))]
public class SpellEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target object (the current instance of the Spell ScriptableObject)
        Spell spell = (Spell)target;

        // Display the default inspector UI (fields)
        DrawDefaultInspector();

        // Display the spell icon (if it exists) in the Inspector
        if (spell.SpellIcon != null)
        {
            GUILayout.Label("Spell Icon:");
            GUILayout.Label(spell.SpellIcon.texture, GUILayout.Width(64), GUILayout.Height(64));
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
