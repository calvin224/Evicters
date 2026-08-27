using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 0.15f;

    private Rigidbody rb;
    private Camera playerCamera;

    private Vector2 moveInput;
    private float xRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            moveInput = Keyboard.current.wKey.isPressed ? Vector2.up : Vector2.zero;

            if (Keyboard.current.sKey.isPressed)
                moveInput.y = -1;

            if (Keyboard.current.aKey.isPressed)
                moveInput.x = -1;

            if (Keyboard.current.dKey.isPressed)
                moveInput.x = 1;
        }

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            // Turn left/right
            transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity);

            // Look up/down
            xRotation -= mouseDelta.y * mouseSensitivity;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            playerCamera.transform.localRotation =
                Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    private void FixedUpdate()
    {
        Vector3 direction =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        direction.Normalize();

        rb.MovePosition(
            rb.position + direction * moveSpeed * Time.fixedDeltaTime
        );
    }
}