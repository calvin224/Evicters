using UnityEngine;
using UnityEngine.InputSystem;

public class PickupObject : Interactable
{
    public float holdDistance = 2f;
    public float throwForce = 12f;

    private Rigidbody rb;
    private Transform playerCamera;
    private bool isHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void Interact()
    {
        if (!isHeld)
            Pickup();
        else
            Drop();
    }

    private void Pickup()
    {
        playerCamera = Camera.main.transform;

        isHeld = true;

        rb.useGravity = false;
        rb.linearDamping = 10f;
        rb.angularDamping = 10f;
    }

    private void Drop()
    {
        isHeld = false;

        rb.useGravity = true;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.05f;

        playerCamera = null;
    }

    private void Throw()
    {
        isHeld = false;

        rb.useGravity = true;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.05f;

        rb.linearVelocity =
            playerCamera.forward * throwForce;

        playerCamera = null;
    }

    private void Update()
    {
        if (!isHeld)
            return;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Throw();
        }
    }

    private void FixedUpdate()
    {
        if (!isHeld || playerCamera == null)
            return;

        Vector3 targetPosition =
            playerCamera.position +
            playerCamera.forward * holdDistance;

        rb.MovePosition(targetPosition);
    }
}