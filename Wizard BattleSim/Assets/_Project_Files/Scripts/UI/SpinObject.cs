using UnityEngine;

public class SpinObject : MonoBehaviour
{
    
    public float speed = 10f;
    public Vector3 direction = Vector3.up;

    void FixedUpdate()
    {
        transform.Rotate(direction, speed * Time.deltaTime);
    }


}
