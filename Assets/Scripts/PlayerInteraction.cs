using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;

    private Camera playerCamera;

    public DialogueManager dialogueManager;

    public bool IsDialogueOpen()
    {
        return FindFirstObjectByType<DialogueManager>() != null &&
               FindFirstObjectByType<DialogueManager>().IsOpen();
    }

    private void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        if (dialogueManager != null && dialogueManager.IsOpen())
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        Debug.DrawRay(
            playerCamera.transform.position,
            playerCamera.transform.forward * interactionDistance,
            Color.red,
            2f
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);

            Interactable interactable =
                hit.collider.GetComponentInParent<Interactable>();

            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
}