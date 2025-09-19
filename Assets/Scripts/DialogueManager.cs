using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public Text nameText;
    public Text dialogueText;
    private Queue<DialogueLine> lines;
    private SceneDetails details;
    public float typingSpeed = 0.005f;

    public enum DIALOGUESTATES
    {
        Writing,
        Waiting,
        Ready,
        Done,
    }

    private DIALOGUESTATES dialogueState;
    private InputAction advance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        lines = new Queue<DialogueLine>();
        advance = new InputAction(binding: "<Keyboard>/enter");
        advance.performed += ctx => PlayNextLine();
        advance.Enable();
    }

    public void TriggerDialogue(GameObject scene)
    {
        details = scene.GetComponent<SceneDetails>(); //The manager yoinks the details from the scene obj we are passing.
        if (details == null)
        {
            Debug.LogWarning("No SceneDetails found on " + scene.name);
            return;
        }
        if (details.dialogueLines == null)
        {
            Debug.LogWarning("No Dialogue found on " + scene.name);
            return;
        }

        lines.Clear();
        foreach (DialogueLine dialogueLine in details.dialogueLines) //And then uses those details to generate text
        {
            lines.Enqueue(dialogueLine);
        }
        dialogueState = DIALOGUESTATES.Ready;// Telling the manager that are lines are all ready
        PlayNextLine();
    }
    void PlayNextLine()
    {
        if (lines.Count == 0){
            dialogueState = DIALOGUESTATES.Done;
            return;
        }

        DialogueLine currentLine = lines.Dequeue();

        nameText.text = currentLine.characterName;

        StartCoroutine(TypeSentence(currentLine));
    }

    IEnumerator TypeSentence(DialogueLine dialogueLine)
    {
        dialogueText.text = "";
        foreach (char letter in dialogueLine.line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
