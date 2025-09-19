using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public Sprite characterIcon;
    [TextArea(3,10)]
    public string line;
}

public class SceneDetails : MonoBehaviour
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
//This is just here for testing purposes at the moment, we will need more serious dialogue triggers later on.
    void Start()
    {
        DialogueManager.Instance.TriggerDialogue(this.gameObject);
    }
}
