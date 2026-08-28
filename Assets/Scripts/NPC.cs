using UnityEngine;

public class NPC : Interactable
{
    public string npcName = "Dave";

    public DialogueManager dialogueManager;
    public NPCAI npcAI;

    private bool hasRefused;

    public override void Interact()
    {
        // If Dave has already refused to leave,
        // interacting with him again evicts him.
        if (hasRefused)
        {
            Evict();
            return;
        }

        StartConversation();
    }

    public void RespondToKnock()
    {
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

        string[] lines =
        {
            "Who is it?",
            "What do you want?",
            "I'm not leaving this house."
        };

        dialogueManager.StartDialogue(
            npcName,
            lines,
            OnDialogueFinished
        );
    }

    private void OnDialogueFinished()
    {
        hasRefused = true;

        Debug.Log(
            npcName +
            " has refused to leave."
        );
    }

    public void ReactToPush()
    {
        if (dialogueManager == null)
            return;

        if (npcAI != null &&
            npcAI.currentState == NPCAI.State.Evicted)
            return;

        string[] lines =
        {
            "Hey!",
            "Don't push me!"
        };

        dialogueManager.StartDialogue(
            npcName,
            lines
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
            npcName
        );

        npcAI.Evict();
    }
}
