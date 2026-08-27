using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text speakerText;
    public TMP_Text dialogueText;

    private string[] lines;
    private int currentLine;

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(string speaker, string[] dialogueLines)
    {
        lines = dialogueLines;
        currentLine = 0;

        speakerText.text = speaker;
        dialogueText.text = lines[currentLine];

        dialoguePanel.SetActive(true);
    }

    public bool IsOpen()
    {
        return dialoguePanel.activeSelf;
    }

    public void NextLine()
    {
        currentLine++;

        if (currentLine >= lines.Length)
        {
            EndDialogue();
            return;
        }

        dialogueText.text = lines[currentLine];
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!dialoguePanel.activeSelf)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            NextLine();
        }
    }
}