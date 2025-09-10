using UnityEngine;
using UnityEngine.InputSystem;

public class PointerRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 100f;

    [Header("Joystick Input")]
    public InputActionProperty moveAction;

    void Update()
    {
        if (moveAction != null && moveAction.action != null)
        {
            Vector2 input = moveAction.action.ReadValue<Vector2>();
            float horizontal = input.x;

            if (Mathf.Abs(horizontal) > 0.1f) // deadzone
            {
                transform.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime);
            }
        }
    }
}
