using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class UnJaden : MonoBehaviour
{
    public List<GameObject> ObjectsToSwap;
    public GameObject ObjectToSwapTo;
    public List<Transform> transforms;

    public void getRefLocations()
    {
        transforms.Clear();
        foreach (GameObject obj in ObjectsToSwap)
        {
            if (obj != null)
            {
                transforms.Add(obj.transform);
            }
        }
    }

    public void SwapLocations()
    {
        // Instantiate new objects at the reference locations
        for (int i = 0; i < ObjectsToSwap.Count; i++)
        {
            var newObject = Instantiate(ObjectToSwapTo, transforms[i].position, transforms[i].rotation);
            newObject.name = ObjectToSwapTo.name;
        }

        Debug.Log("Swapped all objects");

        // Destroy the old objects
        Debug.Log("Deleting Old Objects");
        for (int i = 0; i < ObjectsToSwap.Count; i++)
        {
            if (ObjectsToSwap[i] != null)
            {
                // Destroy old objects immediately in the editor
                DestroyImmediate(ObjectsToSwap[i]);

            }
        }

        // Clear the references
        transforms.Clear();
        ObjectsToSwap.Clear();


    }
}
