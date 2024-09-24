using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(UnJaden))]
public class UnJaden_CustomEditor : Editor
{

        public override void OnInspectorGUI()
    {
        UnJaden unJaden = (UnJaden)target;

        base.OnInspectorGUI();

        //adda a Header to the inspector
        GUILayout.Label("The UnJaden Script hehe", EditorStyles.boldLabel);

        // Add a button to the inspector
        
        if (GUILayout.Button("Get Reference Locations"))
        {
            unJaden.getRefLocations();
        }


        if (GUILayout.Button("Swap Locations"))
        {
            unJaden.SwapLocations();
        }
    }
}
