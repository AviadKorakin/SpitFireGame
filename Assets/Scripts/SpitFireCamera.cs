using UnityEngine;

public class SpitFireCamera : MonoBehaviour
{
    public float rotationSpeed=1f; // Sensitivity for rotation speed
    public float verticalAngleLimit; // Limit for vertical rotation
    public float horizontalAngleLimit; // Limit for horizontal rotation

    private float horizontalRotation; // Current horizontal rotation
    private float verticalRotation; // Current vertical rotation

    private float deltaTimeFactor = 100f;

    void Start()
    {
        // Initialize values
        horizontalRotation = 0f;
        verticalRotation = 0f;
        rotationSpeed = 1f;
        verticalAngleLimit = 40f;
        horizontalAngleLimit = 90f;
    }

    void Update()
    {
        // Get mouse input for rotation, adjusted with deltaTime for smooth control
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime* deltaTimeFactor;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime* deltaTimeFactor;

        // Update and clamp horizontal rotation within the specified limits
        horizontalRotation = Mathf.Clamp(horizontalRotation + mouseX, -horizontalAngleLimit, horizontalAngleLimit);

        // Update and clamp vertical rotation within the specified limits
        verticalRotation = Mathf.Clamp(verticalRotation - mouseY, -verticalAngleLimit, verticalAngleLimit);

        // Apply the clamped rotation directly to the camera's transform
        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
    }
}
