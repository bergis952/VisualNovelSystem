using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using Ink.UnityIntegration;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    [Header("UI Elements")]
    public Text nameText;
    public Text dialogueText;
    [Header("Text Preferances")]
    public float typingSpeed = 0.005f;
    [Header("Ink Stuff")]
    public TextAsset inkJSONAsset;
    private Queue<DialogueLine> lines;
    private SceneDetails details;

    private Story inkStory;

    private enum DIALOGUESTATES
    {
        Writing,//Currently typing out the text
        Ready,//Done typing, waiting for the player's next input
        Done,//Done typing, no more lines in the scene
    }
    private DIALOGUESTATES dialogueState;
    private InputAction advance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        advance = new InputAction(binding: "<Keyboard>/enter");//Sets up are "go on" input. Currently only enter.
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
        
        foreach (DialogueLine dialogueLine in details.dialogueLines) //And then uses those details to generate text
        {
            inkStory = new Story(inkJSONAsset.text);
        }
        dialogueState = DIALOGUESTATES.Ready;// Telling the manager that are lines are all ready
        PlayNextLine();
    }
    void PlayNextLine()
    {
        if (inkStory.canContinue){
            dialogueState = DIALOGUESTATES.Ready;

            string nextLine = inkStory.Continue();

            FetchCharacterDetails();

            StartCoroutine(TypeSentence(nextLine));
        }
    }

    void FetchCharacterDetails(){
        nameText.text = "";

        foreach (string inkTag in inkStory.currentTags){

            string[] split = inkTag.Split(":");
            string key = split[0].Trim();
            string value = split[1].Trim();

            if (key == "character"){
                nameText.text = value;
            }
        }
    }

    IEnumerator TypeSentence(string line)
    {
        dialogueText.text = "";
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
