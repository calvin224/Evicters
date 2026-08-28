using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text speakerText;
    public TMP_Text dialogueText;

    private string[] lines;
    private int currentLine;

    private bool justOpened;

    private Action dialogueFinished;

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(
        string speaker,
        string[] dialogueLines,
        Action onFinished = null)
    {
        if (dialogueLines == null ||
            dialogueLines.Length == 0)
        {
            Debug.LogWarning("Dialogue has no lines.");
            return;
        }

        lines = dialogueLines;
        currentLine = 0;

        dialogueFinished = onFinished;

        speakerText.text = speaker;
        dialogueText.text = lines[currentLine];

        dialoguePanel.SetActive(true);

        // Prevent the E used to start the dialogue
        // from immediately advancing the first line.
        justOpened = true;
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

        justOpened = false;

        Action callback = dialogueFinished;
        dialogueFinished = null;

        callback?.Invoke();
    }

    private void Update()
    {
        if (!dialoguePanel.activeSelf)
            return;

        if (justOpened)
        {
            justOpened = false;
            return;
        }

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            NextLine();
        }
    }
}