using UnityEngine;

public class Door : Interactable
{
    public float openAngle = 90f;
    public float openSpeed = 5f;
    public NPC occupant;

    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    public override void Interact()
    {
        if (occupant != null)
        {
            occupant.RespondToKnock();
        }
        else
        {
            Debug.Log("Door has no occupant assigned!");
        }

        isOpen = !isOpen;
    }

    private void Update()
    {
        Quaternion targetRotation = isOpen
            ? openRotation
            : closedRotation;

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * openSpeed
        );
    }
}