using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using Ink.UnityIntegration;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    [Header("UI Elements")]
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image rightPortrait;
//Temporary Demo stuff
    public Sprite standard;
    public Sprite happy;
    public Sprite angry;

    [Header("Text Preferances")]
    public float typingSpeed = 0.005f;
    [Header("Ink Stuff")]
    public TextAsset inkJSONAsset;

    private Story inkStory;

    private enum DIALOGUESTATES{
        Writing,//Currently typing out the text
        Ready,//Done typing, waiting for the player's next input
        Done,//Done typing, no more lines in the scene
    }
    private DIALOGUESTATES dialogueState;
    private InputSystem_Actions actions;

    void Awake()
    {
        if (Instance == null){
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    

        actions = new InputSystem_Actions();

        actions.Dialogue.Advance.performed += ctx => PlayNextLine();
        TriggerDialogue();
    }
    void OnEnable()
    {
        actions.Dialogue.Enable();
    }

    void OnDisable()
    {
        actions.Dialogue.Disable();
    }
    public void TriggerDialogue()
    {
        inkStory = new Story(inkJSONAsset.text);
        dialogueState = DIALOGUESTATES.Ready;
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

    void FetchCharacterDetails()
    {
        nameText.text = "";

        foreach (string inkTag in inkStory.currentTags){

            string[] split = inkTag.Split(":");
            string key = split[0].Trim();
            string value = split[1].Trim();

            if (key == "character"){
                nameText.text = value;
            }

            else if (key == "portrait"){
                SetPortrait(value);
            }
        }
    }

    void SetPortrait(string pKey)
    {
        if (pKey == "happy"){
            rightPortrait.sprite = happy;
        }
        else if (pKey == "angry"){
            rightPortrait.sprite = angry;
        }
        else 
        {
            rightPortrait.sprite = standard;
        }
    }

    IEnumerator TypeSentence(string line)
    {
        dialogueText.text = "";
        foreach (char letter in line.ToCharArray()){
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
