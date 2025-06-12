using UnityEngine;

public class RemoveTaggedCollidersTool : MonoBehaviour
{
    [Tooltip("Only objects with this tag will have their colliders removed.")]
    public string targetTag = "Grass";

    public void RemoveTaggedColliders()
    {
        int count = 0;
        Collider[] allColliders = GetComponentsInChildren<Collider>(true);

        foreach (Collider col in allColliders)
        {
            if (col != null && col.gameObject.CompareTag(targetTag))
            {
                DestroyImmediate(col);
                count++;
            }
        }

        Debug.Log($"Removed {count} colliders from objects tagged '{targetTag}'.");
    }
}
