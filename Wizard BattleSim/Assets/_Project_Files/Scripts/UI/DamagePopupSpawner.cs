using TMPro;
using Unity.Netcode;
using UnityEngine;

public class DamagePopupSpawner : NetworkBehaviour
{
    public static DamagePopupSpawner Instance;
    [Header("Assign a NON-networked prefab with TextMeshPro as child 0")]
    public GameObject Prefab;

    private void Awake()
    {
        // Simple singleton that survives scene loads
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Call this from anywhere (client or server). It will route correctly.
    /// </summary>
    public void CreatePopUp(Vector3 position, string text, Color color)
    {


        // If we're the server, just broadcast to everyone.
        if (IsServer)
        {
            ShowPopupClientRpc(position, text, (Color32)color, Random.Range(-1f, 1f));
        }
        else
        {
            // Ask the server to broadcast it.
            CreatePopupServerRpc(position, text, (Color32)color, Random.Range(-1f, 1f));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreatePopupServerRpc(Vector3 position, string text, Color32 color, float jitter)
    {
        // Fan out to all clients (including the one who asked)
        ShowPopupClientRpc(position, text, color, jitter);
    }

    [ClientRpc]
    private void ShowPopupClientRpc(Vector3 position, string text, Color32 color, float jitter)
    {
        if (Prefab == null) { Debug.LogWarning("DamagePopupSpawner Prefab not assigned."); return; }

        // Local-only instantiate (NO NetworkObject required/used)
        var popup = Instantiate(Prefab, position, Quaternion.identity);

        // Small horizontal jitter so every client sees the same offset
        popup.transform.position += popup.transform.right * jitter;

        // Assumes child 0 has TextMeshPro
        var tmp = popup.transform.GetChild(0).GetComponent<TextMeshPro>();
        tmp.text = text;
        tmp.faceColor = color;
        tmp.color = color;

        Destroy(popup, 1f);
    }
}
