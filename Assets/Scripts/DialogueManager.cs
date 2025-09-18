using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void TriggerDialogue(GameObject scene)
    {
        SceneDetails details = scene.GetComponent<SceneDetails>(); //The manager yoinks the details from the scene obj we are passing.
        if (details == null || details.dialogue == null)
        {
            Debug.LogWarning("No SceneDetails or Dialogue found on: " + scene.name);
            return;
        }

        foreach (DialogueLine dialogueLine in details.dialogue.dialogueLines) //And then uses those details to generate text
        {
            Debug.Log(dialogueLine.characterName + ": " + dialogueLine.line);
        }
    }
}
