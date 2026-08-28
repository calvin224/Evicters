using UnityEngine;

public class Door : Interactable
{
    public float openAngle = 90f;
    public float openSpeed = 5f;

    [Header("Occupant")]
    public NPC occupant;
    public Transform occupantPoint;

    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        closedRotation = transform.localRotation;

        openRotation =
            closedRotation *
            Quaternion.Euler(0f, openAngle, 0f);
    }

    public override void Interact()
    {
        Debug.Log("KNOCK - Door.Interact()");

        if (occupant == null)
        {
            Debug.LogWarning(
                "No occupant assigned to the door."
            );

            return;
        }

        if (occupantPoint == null)
        {
            Debug.LogError(
                "Occupant Point is not assigned to the door."
            );

            return;
        }

        // Do NOT open the door.
        // Just knock and tell the occupant.
        occupant.RespondToKnock(occupantPoint);
    }

    public void OpenDoor()
    {
        Debug.Log("OPEN DOOR CALLED");

        isOpen = true;
    }

    public void CloseDoor()
    {
        Debug.Log("CLOSE DOOR CALLED");

        isOpen = false;
    }

    private void Update()
    {
        Quaternion targetRotation =
            isOpen
                ? openRotation
                : closedRotation;

        transform.localRotation =
            Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * openSpeed
            );
    }
}