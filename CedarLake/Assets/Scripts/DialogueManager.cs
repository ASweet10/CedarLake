using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    GameController gameController;
    FirstPersonController firstPersonController;
    FirstPersonHighlights fpHighlights;
    MouseLook mouseLook;

    [SerializeField] GameObject dialogueParent;
    [SerializeField] GameObject playerCarRef;
    [SerializeField] TMP_Text speakerText;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] Button option1Button;
    [SerializeField] Button option2Button;
    [SerializeField] Button option3Button;

    List<dialogueString> dialogueList;


    [Header("Zoom")]
    [SerializeField] Camera mainCamera;
    [SerializeField] float zoomTime = 0.3f;

    int currentDialogueIndex = 0;
    bool optionSelected = false;
    float typingSpeed = 0.04f;
    [SerializeField] AudioSource dialogueClickAudio;

    void Start() {
        dialogueParent.SetActive(false); // Hide dialogue by default
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        firstPersonController = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
        fpHighlights = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonHighlights>();
        mouseLook = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MouseLook>();
    }

    public void PlayDialogueClickSFX() {
        if (!dialogueClickAudio.isPlaying) {
            dialogueClickAudio.Play();
        }
    }

    public void DialogueStart(List<dialogueString> textToPrint, string speakerName) {
        dialogueParent.SetActive(true);
        speakerText.text = speakerName;
        dialogueList = textToPrint;

        firstPersonController.DisablePlayerMovement(true, true);
        fpHighlights.ClearHighlighted();
        StartCoroutine(HandleZoomIn(true));

        switch(speakerName) {
            case "Cashier":
                if(gameController.playerCaughtStealing) {
                    currentDialogueIndex = 9; // Steal options
                } else if(gameController.hasPurchasedGas) {
                    currentDialogueIndex = 10; // No item in hand
                } else {
                    currentDialogueIndex = 0; // Can I help you?
                }
                break;
            case "AJ":
                break;
            case "David":
                break;
            case "Hunter":
                if(gameController.hunterWarningComplete) {
                    currentDialogueIndex = 16; // Random hunter options
                } else {
                    currentDialogueIndex = 0; // Hunter warning start
                }
                break;
        }
        DisableButtons();
        StartCoroutine(PrintDialogue());
    }

    IEnumerator PrintDialogue() {
        while (currentDialogueIndex < dialogueList.Count) {
            dialogueString line = dialogueList[currentDialogueIndex];

            line.startDialogueEvent?.Invoke();

            if(line.isRandomOption) {
                yield return StartCoroutine(TypeText(line.randomOptions[Random.Range(0, line.randomOptions.Length)]));
            }

            if (line.isQuestion) {
                yield return StartCoroutine(TypeText(line.text));

                // check number of answers? (put in array instead of 3 strings)
                // Only one box appears / is interactable if only one answer etc.
                option1Button.interactable = true;
                option2Button.interactable = true;
                option3Button.interactable = true;

                /*
                option1Button.GetComponentInChildren<DialogueButtonHover>().enabled = true;
                option2Button.GetComponentInChildren<DialogueButtonHover>().enabled = true;
                option3Button.GetComponentInChildren<DialogueButtonHover>().enabled = true;
                */

                option1Button.GetComponentInChildren<TMP_Text>().text = line.answerOption1;
                option2Button.GetComponentInChildren<TMP_Text>().text = line.answerOption2;
                option3Button.GetComponentInChildren<TMP_Text>().text = line.answerOption3;

                option1Button.onClick.AddListener(() => HandleOptionSelected(line.option1IndexJump));
                option2Button.onClick.AddListener(() => HandleOptionSelected(line.option2IndexJump));
                option3Button.onClick.AddListener(() => HandleOptionSelected(line.option3IndexJump));

                yield return new WaitUntil(() => optionSelected);
            
            } else {
                yield return StartCoroutine(TypeText(line.text));
            }

            line.endDialogueEvent?.Invoke();
            optionSelected = false;
        }
        DialogueStop();
    }
    private IEnumerator TypeText(string text) {

        dialogueText.text = "";
        foreach(char letter in text.ToCharArray()) {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        if(!dialogueList[currentDialogueIndex].isQuestion) {
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }
        if(dialogueList[currentDialogueIndex].isEnd) {
            DialogueStop();
        }
        currentDialogueIndex++;
    }
    void HandleOptionSelected(int indexJump) {
        optionSelected = true;
        DisableButtons();

        currentDialogueIndex = indexJump;
    }
    void DisableButtons() {
        option1Button.interactable = false;
        option2Button.interactable = false;
        option3Button.interactable = false;

        option1Button.GetComponentInChildren<TMP_Text>().text = "";
        option2Button.GetComponentInChildren<TMP_Text>().text = "";
        option3Button.GetComponentInChildren<TMP_Text>().text = "";

        /*
        option1Button.GetComponentInChildren<DialogueButtonHover>().enabled = false;
        option2Button.GetComponentInChildren<DialogueButtonHover>().enabled = false;
        option3Button.GetComponentInChildren<DialogueButtonHover>().enabled = false;
        */
    }

    public void DialogueStop() {
        StopAllCoroutines();
        dialogueText.text = "";
        dialogueParent.SetActive(false);

        firstPersonController.enabled = true;
        mouseLook.CanRotateMouse = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(HandleZoomIn(false));
        firstPersonController.DisablePlayerMovement(false, false);
    }
    IEnumerator HandleZoomIn(bool shouldZoomIn) {
        float targetFOV = shouldZoomIn ? 50 : 60;
        float startFOV = mainCamera.fieldOfView;
        float timeElapsed = 0;

        while(timeElapsed < zoomTime) {
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, timeElapsed / zoomTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.fieldOfView = targetFOV;
        yield return null;
    }

    public void PurchaseGas() {
        gameController.hasPurchasedGas = true;
        playerCarRef.tag = "Head To Park";
    }
    public void FinishHunterWarning() {
        gameController.hunterWarningComplete = true;
    }
    public void DisableCaughtStealingStatus(){
        gameController.playerCaughtStealing = false;
    }
}