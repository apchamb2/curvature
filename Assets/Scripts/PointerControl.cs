using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class PointerRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 50f;

    [Tooltip("Maximum left/right angle in degrees from the starting rotation")]
    public float maxYawAngle = 90f;

    [Header("References")]
    public Transform sphere;  // the sphere at the triangle vertex

    [Header("Joystick Input")]
    public InputActionProperty rightJoystickAction;

    [Header("Button Input")]
    public InputActionProperty aButtonAction;   // A button (use XRI RightHand Interaction / Activate)

    [Header("Experiment Settings")]
    public string subjectID = "Subject001";  // name for the CSV file

    private float currentYaw = 0f;  // Tracks total yaw relative to start
    private string filePath;

    void Start()
    {
        // File will be saved as <subjectID>.csv
        string fileName = subjectID + ".csv";
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        // Write header if file does not exist
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Timestamp,Angle\n");
        }

        Debug.Log($"Logging to: {filePath}");
    }

    void Update()
    {
        // --- Joystick Rotation ---
        if (rightJoystickAction != null && rightJoystickAction.action != null)
        {
            Vector2 input = rightJoystickAction.action.ReadValue<Vector2>();
            float horizontal = input.x;

            if (Mathf.Abs(horizontal) > 0.1f) // deadzone
            {
                float deltaYaw = horizontal * rotationSpeed * Time.deltaTime;
                float newYaw = Mathf.Clamp(currentYaw + deltaYaw, -maxYawAngle, maxYawAngle);
                float appliedDelta = newYaw - currentYaw;

                transform.Rotate(Vector3.up, appliedDelta, Space.World);
                currentYaw = newYaw;
            }
        }

        // --- Save angle when pressing A button ---
        if (aButtonAction != null && aButtonAction.action != null)
        {
            if (aButtonAction.action.WasPressedThisFrame())
            {
                if (sphere != null)
                {
                    // Pointer forward direction
                    Vector3 pointerForward = transform.forward;

                    // Direction from pointer to sphere
                    Vector3 legDirection = (sphere.position - transform.position).normalized;

                    // Compute signed angle in horizontal plane (Y axis up)
                    float angle = Vector3.SignedAngle(pointerForward, legDirection, Vector3.up);

                    // Append to CSV
                    string line = $"{System.DateTime.Now:O},{angle:F2}\n";
                    File.AppendAllText(filePath, line);

                    Debug.Log($"Saved Angle: {angle:F2} degrees to {filePath}");
                }
                else
                {
                    Debug.LogWarning("Sphere reference not assigned!");
                }
            }
        }
    }
}
