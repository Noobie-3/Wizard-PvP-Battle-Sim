using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Wand))]
public class Wand_editorScript : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target object (the current instance of the Wand ScriptableObject)
        Wand Wand = (Wand)target;

        // Display the default inspector UI (fields)
        DrawDefaultInspector();

        // Display the Wand icon (if it exists) in the Inspector
        if (Wand.Icon != null)
        {
            GUILayout.Label("Wand Icon:");
            GUILayout.Label(Wand.Icon.texture, GUILayout.Width(64), GUILayout.Height(64));
        }
    }

    // This method allows us to display the custom icon in the Project Window
    [InitializeOnLoadMethod]
    static void Initialize()
    {
        // Hook into the Project Window GUI event to draw custom icons
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    // Draws the Wand icon in the Project View, showing both the icon and the name
    static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        // Get the asset path and load the corresponding ScriptableObject
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var Wand = AssetDatabase.LoadAssetAtPath<Wand>(path);

        // If the asset is a Wand and has an icon, draw the custom icon
        if (Wand != null && Wand.Icon != null)
        {
            // Create a Rect for the custom icon, aligned to the left
            Rect iconRect = new Rect(selectionRect.x, selectionRect.y - 5, 64, 64); // Smaller 16x16 icon on the left side

            // Draw the custom icon within this smaller Rect
            GUI.DrawTexture(iconRect, Wand.Icon.texture);

            // The name will be automatically displayed next to the icon by Unity
        }
    }
}
