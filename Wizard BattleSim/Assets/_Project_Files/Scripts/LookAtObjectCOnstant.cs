using UnityEngine;

public class LookAtObjectCOnstant : MonoBehaviour
{
    public Transform Target;

    private void FixedUpdate()
    {
        Vector3 TargetDir = new Vector3(Target.position.x, 0f, Target.position.z);
            gameObject.transform.LookAt(TargetDir);

    }
}
