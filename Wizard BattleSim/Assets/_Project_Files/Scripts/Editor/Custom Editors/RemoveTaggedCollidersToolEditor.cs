using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RemoveTaggedCollidersTool))]
public class RemoveTaggedCollidersToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RemoveTaggedCollidersTool tool = (RemoveTaggedCollidersTool)target;
        if (GUILayout.Button(" Remove Colliders From Tagged Objects"))
        {
            tool.RemoveTaggedColliders();
        }
    }
}
