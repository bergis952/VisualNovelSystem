using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using Ink.UnityIntegration;
using DG.Tweening;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public GameObject[] charactersInScene;
    [Header("UI Elements")]
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Canvas uiCanvas;
    public Canvas charCanvas;

    public Transform farLeftCharacterLocation;
    public Transform leftCharacterLocation;
    public Transform centerCharacterLocation;
    public Transform rightCharacterLocation;
    public Transform farRightCharacterLocation;

    public GameObject buttonPrefab;
    public VerticalLayoutGroup buttonContainer;
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

//-------------------- CORE DIALOGUE LOOP --------------------
    public void TriggerDialogue()
    {
        inkStory = new Story(inkJSONAsset.text);//Converts the Inky JSON into "inkStory" which is the variable that is used for all story-related stuff
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
        //clears our text field of the previous line's information
        nameText.text = "";
        string characterName = "";

        foreach (string inkTag in inkStory.currentTags){
            string[] split = inkTag.Split(":");
            //Ink tags work like: "character: Mario"
            //                    "effect: Spawn"
            //So we split them into an array of [character:Mario, effect:Spawn]
            string key = split[0].Trim();
            string value = split[1].Trim();

            if (key == "character"){//This one is the easiest, so I didn't give it its own function.
                characterName = value;
                nameText.text = characterName;
            }
            else if (key == "action"){
                TriggerAction(value, characterName);
            }
            else if (key == "expression" && characterName != ""){//changes the portrait shown based on the character tag and expression tag
                SetPortrait(value, characterName);
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
        if (inkStory.currentChoices.Count >= 1){//Puts us into a choice state if needed.
            dialogueState = DIALOGUESTATES.Waiting;
            DisplayChoices();
        }
        else
        {
            dialogueState = DIALOGUESTATES.Ready;
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

                button.GetComponent<Button>().onClick.AddListener(() => OnClickButton(choice));
            }
        }
    }
//-------------------- HANDLING PLAYER INPUT --------------------
    public void OnClickButton(Choice choice)
    {
        inkStory.ChooseChoiceIndex(choice.index);
        if (buttonContainer != null)
        {
            foreach (var btn in buttonContainer.GetComponentsInChildren<Button>())
            {
                Destroy(btn.gameObject);
            }
            dialogueState = DIALOGUESTATES.Ready;
            PlayNextLine();
        }
    }
//-------------------- VISUALS AND EFFECTS USING INK TAGS --------------------
    void TriggerAction(string value, string character)
    {
        
        string[] parts = value.Split(' ');
        //We're once again splitting a bit from Inky.
        //The action key gives us a long string, such as "enter far_left right 1.2" (enter FROM far left TO right at speed 1.2) which we split, giving us:             
        string moveType = parts[0].Trim();      // moveType === "enter"
        string from = parts [1].Trim();         //from === "far_left"
        string to = parts [2].Trim();           //to === "right"
        float speed = float.Parse(parts[3]);   //speed = 1.2f

        GameObject characterPrefab = System.Array.Find(charactersInScene, obj => obj.GetComponent<CharacterDetails>().characterName == character);
        CharacterMove(characterPrefab, from, to, speed);
    }

    void CharacterMove(GameObject characterPrefab, string from, string to, float speed){
        Transform fromSpot = GetTransformByKeyword(from);
        Transform toSpot = GetTransformByKeyword(to);
        GameObject characterInstance = GameObject.Find(characterPrefab.name + "(Clone)");

        //Either finds our character in the scene or creates them at the designated (from) spot
        if (characterInstance == null){
            characterInstance = Instantiate(characterPrefab, charCanvas.transform, false);
            characterInstance.name = characterPrefab.name + "(Clone)";
        }
        RectTransform rect = characterInstance.GetComponent<RectTransform>();
        RectTransform fromRect = fromSpot.GetComponent<RectTransform>();
        RectTransform toRect = toSpot.GetComponent<RectTransform>();

        rect.anchoredPosition = fromRect.anchoredPosition;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        //Moves the character to the desired location
        rect.DOAnchorPos(toRect.anchoredPosition, speed).SetEase(Ease.InOutCubic);
    }
    void SetPortrait(string expression, string character)
    {
        GameObject characterPrefab = System.Array.Find(charactersInScene, obj => obj.GetComponent<CharacterDetails>().characterName == character);
        GameObject characterInstance = GameObject.Find(characterPrefab.name + "(Clone)");

        characterInstance.GetComponent<Animator>().SetTrigger(expression);
    }
    Transform GetTransformByKeyword(string keyword)
    {
        switch (keyword.ToLower()) {
            case "left": return leftCharacterLocation;
            case "far_left": return farLeftCharacterLocation;
            case "center": return centerCharacterLocation;
            case "right": return rightCharacterLocation;
            case "far_right": return farRightCharacterLocation;
            default: return null;
        }
    }
}
