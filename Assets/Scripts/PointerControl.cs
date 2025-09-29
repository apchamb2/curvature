using UnityEngine;
using UnityEngine.InputSystem;

public class PointerRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 50f;

    [Header("Joystick Input")]
    public InputActionProperty rightJoystickAction; 

    void Update()
    {
        if (rightJoystickAction != null && rightJoystickAction.action != null)
        {
            Vector2 input = rightJoystickAction.action.ReadValue<Vector2>();
            float horizontal = input.x;

            if (Mathf.Abs(horizontal) > 0.1f)
            {
                transform.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime, Space.World);
            }
        }
    }
}
