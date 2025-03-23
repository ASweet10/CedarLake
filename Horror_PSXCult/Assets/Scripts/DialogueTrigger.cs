using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] string speakerName;
    [SerializeField] List<dialogueString> dialogueStrings = new List<dialogueString>();
    DialogueManager dialogueManager;
    void Start() {
        dialogueManager = GameObject.FindGameObjectWithTag("Player").GetComponent<DialogueManager>();
    }
    public void TriggerDialogue() {
        dialogueManager.DialogueStart(dialogueStrings, speakerName);
    }
}

[System.Serializable]
public class dialogueString {
    public string text;
    public bool isEnd; // Is this final conversation line?

    [Header("Random Option")]
    public bool isRandomOption; // Choose from list
    public string[] randomOptions;

    [Header("Branch")]
    public bool isQuestion;
    public string answerOption1;
    public string answerOption2;
    public string answerOption3;
    public int option1IndexJump;
    public int option2IndexJump;
    public int option3IndexJump;

    [Header("Triggered Events")]
    public UnityEvent startDialogueEvent;
    public UnityEvent endDialogueEvent;
}
