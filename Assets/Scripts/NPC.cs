using UnityEngine;

public class NPC : Interactable
{
    public OccupantData occupantData;

    public DialogueManager dialogueManager;
    public NPCAI npcAI;

    [Header("Door")]
    public Door door;

    private bool hasRefused;

    public override void Interact()
    {
        if (hasRefused)
        {
            Evict();
            return;
        }

        StartConversation();
    }

    public void RespondToKnock(Transform doorPoint)
    {
        Debug.Log(
            occupantData.occupantName +
            " received the knock."
        );

        if (npcAI == null)
        {
            Debug.LogError(
                "NPCAI is not assigned to " +
                gameObject.name
            );

            return;
        }

        npcAI.GoToDoor(
            doorPoint,
            ArrivedAtDoor
        );
    }

    private void ArrivedAtDoor()
    {
        Debug.Log(
            occupantData.occupantName +
            " arrived at the door."
        );

        if (door != null)
        {
            door.OpenDoor();
        }

        StartConversation();
    }

    private void StartConversation()
    {
        if (dialogueManager == null)
        {
            Debug.LogError(
                "DialogueManager is not assigned to " +
                gameObject.name
            );

            return;
        }

        dialogueManager.StartDialogue(
            occupantData.occupantName,
            occupantData.knockDialogue,
            OnDialogueFinished
        );
    }

    private void OnDialogueFinished()
    {
        hasRefused = true;

        Debug.Log(
            occupantData.occupantName +
            " has refused to leave."
        );

        if (door != null)
        {
            door.CloseDoor();
        }

        if (npcAI != null)
        {
            npcAI.LeaveDoor();
        }
    }

    public void ReactToPush()
    {
        if (dialogueManager == null)
            return;

        if (npcAI != null &&
            npcAI.currentState == NPCAI.State.Evicted)
            return;

        dialogueManager.StartDialogue(
            occupantData.occupantName,
            occupantData.pushDialogue
        );

        if (npcAI != null)
        {
            npcAI.BecomeAngry();
        }
    }

    private void Evict()
    {
        if (npcAI == null)
        {
            Debug.LogError(
                "NPCAI is not assigned to " +
                gameObject.name
            );

            return;
        }

        Debug.Log(
            "Evicting " +
            occupantData.occupantName
        );

        npcAI.Evict();
    }
}