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
    public GameObject buttonPrefab;
    public VerticalLayoutGroup buttonContainer;
//Temporary Demo stuff
    public Sprite standard;
    public Sprite happy;
    public Sprite angry;
//End temp stuff
    [Header("Text Preferances")]
    public float typingSpeed = 0.01f;
    [Header("Ink Stuff")]
    public TextAsset inkJSONAsset;//This is the actual story asset. Will eventually be fed in through an event trigger

    private Story inkStory;

    private enum DIALOGUESTATES{
        Writing,//Currently typing out the text
        Waiting,//Done typing, waiting for player to make choice
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
        //This sets up the Input system
        actions = new InputSystem_Actions();
        actions.Dialogue.Advance.performed += ctx => PlayNextLine();//calls the next line any time "advance" inputs are used (lmouse, enter, etc...)
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
        if (dialogueState == DIALOGUESTATES.Ready && inkStory.canContinue){
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

    void DisplayChoices()
    {
        if (buttonContainer.GetComponentsInChildren<Button>().Length > 0)
        {
            Debug.Log("Choices have already been displayed.");
        }
        else
        {
            for (int i = 0; i < inkStory.currentChoices.Count; i++)
            {
                var choice = inkStory.currentChoices[i];
                var button = Instantiate(buttonPrefab);
                button.transform.SetParent(buttonContainer.transform, false);

                var bText = button.GetComponentInChildren<TMP_Text>();
                bText.text = choice.text; 
            }
        }
    }

    IEnumerator TypeSentence(string line)
    {
        dialogueState = DIALOGUESTATES.Writing;
        dialogueText.text = "";
        foreach (char letter in line.ToCharArray()){
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        if (inkStory.currentChoices.Count >= 1){
            dialogueState = DIALOGUESTATES.Waiting;
            DisplayChoices();
        }
        else
        {
            dialogueState = DIALOGUESTATES.Ready;

        }
    }
}
