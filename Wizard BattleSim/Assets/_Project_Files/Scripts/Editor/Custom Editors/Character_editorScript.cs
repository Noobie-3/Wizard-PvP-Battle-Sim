using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Character))]
public class Character_editorScript : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target object (the current instance of the Character ScriptableObject)
        Character Character = (Character)target;

        // Display the default inspector UI (fields)
        DrawDefaultInspector();

        // Display the Character icon (if it exists) in the Inspector
        if (Character.Icon != null)
        {
            GUILayout.Label("Character Icon:");
            GUILayout.Label(Character.Icon.texture, GUILayout.Width(64), GUILayout.Height(64));
        }
    }

    // This method allows us to display the custom icon in the Project Window
    [InitializeOnLoadMethod]
    static void Initialize()
    {
        // Hook into the Project Window GUI event to draw custom icons
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    // Draws the Character icon in the Project View, showing both the icon and the name
    static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        // Get the asset path and load the corresponding ScriptableObject
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var Character = AssetDatabase.LoadAssetAtPath<Character>(path);

        // If the asset is a Character and has an icon, draw the custom icon
        if (Character != null && Character.Icon != null)
        {
            // Create a Rect for the custom icon, aligned to the left
            Rect iconRect = new Rect(selectionRect.x, selectionRect.y - 5, 64, 64); // Smaller 16x16 icon on the left side

            // Draw the custom icon within this smaller Rect
            GUI.DrawTexture(iconRect, Character.Icon.texture);

            // The name will be automatically displayed next to the icon by Unity
        }
    }
}
