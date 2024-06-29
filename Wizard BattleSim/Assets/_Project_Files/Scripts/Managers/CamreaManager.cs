using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamreaManager : MonoBehaviour
{
    public Vector3 offset; // Offset of the camera relative to the player
    public float rotationSpeed = 5f; // Speed at which the camera rotates
    public float followSpeed = 10f; // Speed at which the camera follows the player

    private float horizontalInput;
    private float verticalInput;

    void Start() {
    }

    void Update() {
        // Get mouse input for camera rotation
        horizontalInput = Input.GetAxis("Mouse X");
        verticalInput = Input.GetAxis("Mouse Y");

        RotateCamera();
    }

    void LateUpdate() {
        FollowPlayer();
    }

    private void RotateCamera() {
        // Rotate the player and camera around the Y axis (horizontal input)
        gameController.GC.Player.transform.Rotate(Vector3.up * horizontalInput * rotationSpeed);

        // Rotate the camera around the X axis (vertical input) for looking up and down
        float desiredAngleX = transform.eulerAngles.x - verticalInput * rotationSpeed;
        desiredAngleX = Mathf.Clamp(desiredAngleX, -40, 60); // Clamp the vertical angle to avoid excessive rotation
        transform.rotation = Quaternion.Euler(desiredAngleX, gameController.GC.Player.transform.eulerAngles.y, 0);
    }

    private void FollowPlayer() {
        // Calculate the desired camera position based on the player's position and offset
        Vector3 desiredPosition = gameController.GC.Player.transform.position + offset;
        // Smoothly move the camera towards the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }
}

