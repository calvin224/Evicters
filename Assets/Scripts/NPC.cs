using UnityEngine;

public class NPC : Interactable
{
    public string npcName = "Dave";

    public DialogueManager dialogueManager;

    public override void Interact()
    {
        StartConversation();
    }

    public void RespondToKnock()
    {
        StartConversation();
    }

    private void StartConversation()
    {
        string[] lines =
        {
            "Who is it?",
            "What do you want?",
            "I'm not leaving this house."
        };

        dialogueManager.StartDialogue(npcName, lines);
    }
}