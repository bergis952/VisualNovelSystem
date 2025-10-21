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
    public GameObject[] charactersInScene;
    [Header("UI Elements")]
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Canvas uiCanvas;

    public Transform leftCharacterLocation;
    public Transform centerCharacterLocation;
    public Transform rightCharacterLocation;

    public GameObject buttonPrefab;
    public VerticalLayoutGroup buttonContainer;
    //Temporary Demo stuff
    public Sprite standard;
    public Sprite happy;
    public Sprite angry;
    public float leftOffset;
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
            else if (key == "character_effect"){
                TriggerCharacterEffect(value, characterName);
            }
            else if (key == "expression" && characterName != ""){//changes the portrait shown based on the character tag and expresion tag
                SetPortrait(value);
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
    void TriggerCharacterEffect(string effect, string character)
    {
        GameObject characterPrefab = System.Array.Find(charactersInScene, obj => obj.GetComponent<CharacterDetails>().characterName == character);
        
        switch (effect)
        {
        //Character appears in position without moving
            case "appear_left":
                SpawnCharacter(characterPrefab, leftCharacterLocation);
                break;
            case "appear_center":
                break;
            case "appear_right":
                break;
        //Character sprite enters from the left side of the screen and goes to one of three positions.
            case "enter_left_to_left":
                EnterCharacter(characterPrefab, leftCharacterLocation, "left");
                break;
            case "enter_left_to_center":
                break;
            case "enter_left_to_right":
                break;
        //Character sprite enters from the right side of the screen and goes to one of three positions.
            case "enter_right_to_left":
                break;
            case "enter_right_to_center":
                break;
            case "enter_right_to_right":
                break;
        //Character moves to the noted position
            case "move_to_left":
                break;
            case "move_to_center":
                break;
            case "move_to_right":
                break;
        //Character sprite exits in the noted direction.
            case "exit_left":
                break;
            case "exit_right":
                break;
        //special effects
            case "character_shake":
                break;
        }
    }
    void SetPortrait(string pKey)
    {
        if (pKey == "happy"){
            //rightCharacterLocation.sprite = happy;
        }
        else if (pKey == "angry"){
            //rightCharacterLocation.sprite = angry;
        }
        else 
        {
            //rightCharacterLocation.sprite = standard;
        }
    }

    void SpawnCharacter(GameObject character, Transform spot)
    {
        if (character != null)
        {
            GameObject newChar = Instantiate(character, spot);
            RectTransform rect = newChar.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0f); 
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot     = new Vector2(0.5f, 0f); 
            rect.anchoredPosition = Vector2.zero; 
            rect.localScale = Vector3.one;
        }
    }
    void EnterCharacter(GameObject character, Transform spot, String start)
    {
        if (character != null)
        {
            GameObject newChar = Instantiate(character, spot);
            RectTransform rect = newChar.GetComponent<RectTransform>();

            switch (start){
                case "left":
                    Vector3 offset = new Vector3(leftOffset,0,0);
                    rect.anchoredPosition = offset;
                    break;
            }
             
            rect.anchorMin = new Vector2(0.5f, 0f); 
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot     = new Vector2(0.5f, 0f); 
            rect.localScale = Vector3.one;
        }
    }
}
