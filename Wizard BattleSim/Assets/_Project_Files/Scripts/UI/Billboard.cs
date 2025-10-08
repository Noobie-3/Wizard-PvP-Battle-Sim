using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;

    void LateUpdate()
    {
        if (cam == null || !cam.isActiveAndEnabled)
        {
            // Try to grab any active camera
            Camera[] cams = Camera.allCameras;
            for (int i = 0; i < cams.Length; i++)
            {
                if (cams[i].isActiveAndEnabled)
                {
                    cam = cams[i];
                    break;
                }
            }

            if (cam == null) return; // still nothing, bail out
        }

        // Face toward that camera
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                         cam.transform.rotation * Vector3.up);
    }
}
