using UnityEngine;
public static class MovementHelper
{
    public static Vector3 SmoothDampDirection(Vector3 current, Vector3 target, ref Vector3 velocityRef, float smoothTime)
    {
        return Vector3.SmoothDamp(current, target, ref velocityRef, smoothTime);
    }

    public static float ApplyGravity(float verticalVelocity, float gravity, float deltaTime, bool isGrounded)
    {
        if (isGrounded && verticalVelocity < 0)
            return -2f;
        return verticalVelocity + gravity * deltaTime;
    }

    public static Vector3 CalculateMovement(Vector3 direction, float speed, float verticalVelocity)
    {
        Vector3 movement = direction * speed;
        movement.y = verticalVelocity;
        return movement;
    }

    public static void RotateTowardsMovement(Transform transform, Vector3 moveDirection, float rotationSpeed)
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
