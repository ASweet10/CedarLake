using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    [SerializeField] GameObject playerRef;
    [SerializeField] GameObject pauseMenuUI;
    [SerializeField] GameObject quitGameOptionUI;
    [SerializeField] Interactables interactables;
    [SerializeField] SceneController sceneController;


    [Header("Objectives")]
    [SerializeField] TMP_Text popupText;
    [SerializeField] TMP_Text objectivePopupText;
    [SerializeField] TMP_Text objectiveTextInPauseMenu;
    [SerializeField] FirstPersonController fpController;
    [SerializeField] string[] gameObjectives;


    public bool holdingGasStationItem = false;
    public bool hasPurchasedGas = false;
    public bool playerHasReadCarNote = false;
    public bool playerNeedsFirewood = true;
    public bool tentCompleted = false;
    public bool hasZippo = false;
    public bool hasLighterFluid = false;
    public bool hasCarKeys = false;
    public bool fireStarted = false;
    public bool playerCaughtStealing = false;
    public bool hunterWarningComplete = false;
    public bool gamePaused = false;

    public bool hasDrink = false;
    public int chosenDrinkIndex = 0;
    

    [Header ("Player Death & Checkpoints")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera deathCamera;
    [SerializeField] GameObject playerDeath3DObject;
    [SerializeField] AnimationClip[] playerDeathClips;
    [SerializeField] Animator playerDeathAnimator;
    [SerializeField] GameObject bloodPool;
    [SerializeField] GameObject deathUI;
    [SerializeField] Transform[] restartPositions;
    [SerializeField] GameObject[] killers;


    [Header ("Main Menu")]
    [SerializeField] GameObject mainMenuUI;
    [SerializeField] GameObject optionsMenuUI;
    [SerializeField] GameObject[] expositionUIObjects;

    
    [Header ("Endings")]
    [SerializeField] TMP_Text endingHeader;
    [SerializeField] TMP_Text endingMessage;
    List<string> endingMessages = new List<string>() {
        "You thought about it and realized this is probably a bad idea. You'd rather be at home snuggled up in a blanket.",
        "You managed to escape and immediately alerted the authorities. You tell them that, unfortunately, you weren't able to save your friends.",
        "Most people would focus on saving their own skin and you went back. You are truly a good friend."
    };
    int currentCheckpoint = 0;
    public int currentObjective = 4;

    void Start() {
        //currentObjective = 0;
        //currentCheckpoint = 0;
        if(SceneManager.GetActiveScene().buildIndex != 1) {  // If main menu / ending
            Cursor.lockState = CursorLockMode.None;
            //Cursor.SetCursor(arrowCursor, Vector2.zero, CursorMode.Auto);
            Cursor.visible = true;
        }
        else {
            interactables.enabled = true;
        }
    }

    void Update() {
        //Debug.Log("FPS: " + Application.targetFrameRate);
    }

    public void ReplayFromDeath() {
        playerRef.transform.position = restartPositions[currentCheckpoint].position;
        playerRef.transform.rotation = restartPositions[currentCheckpoint].rotation;
    }

    /**** Menus ****/
    public void OpenQuitGameUI() {
        quitGameOptionUI.SetActive(true);
    }
    public void DeclineQuitGame() {
        quitGameOptionUI.SetActive(false);
    }
    public void ConfirmQuitGame() {
        Application.Quit();
    }

    public void ToggleOptionsMenu(bool toggle) {
        mainMenuUI.SetActive(!toggle);
        optionsMenuUI.SetActive(toggle);
    }
    public void PlayGameButton() {
        StartCoroutine(sceneController.PanCameraOnPlay());
        //StartCoroutine(sceneController.FadeOut(2, 1));
    }
    public void ReturnToMainMenu() {
        SceneManager.LoadScene(0);
    }

    public void ResumeGame() {
        objectiveTextInPauseMenu.enabled = false;
        pauseMenuUI.SetActive(false);
        popupText.enabled = true;
        AudioListener.volume = 1f;
        fpController.DisablePlayerMovement(false, false);
        //Time.timeScale = 1f;
    }
    public void PauseGame() {
        pauseMenuUI.SetActive(true);
        popupText.enabled = false;
        objectiveTextInPauseMenu.enabled = true;
        objectiveTextInPauseMenu.text = gameObjectives[currentObjective];
        AudioListener.volume = 0.3f;
        fpController.DisablePlayerMovement(true, true);
        //Time.timeScale = 0f;
    }
    

    /* Text Display */
    public IEnumerator DisplayPopupMessage(string message) {
        popupText.text = message;
        yield return new WaitForSeconds(3f);
        popupText.text = "";
    }
    public IEnumerator HandleNextObjective() {
        currentObjective ++;
        //objectivePopupText.text = "Next Objective:  " + gameObjectives[currentObjective];
        objectivePopupText.enabled = true;
        yield return new WaitForSeconds(4);
        objectivePopupText.enabled = false;
    }

    public IEnumerator HandlePlayerDeath() {    
        fpController.DisablePlayerMovement(true, true);
        deathCamera.enabled = true;
        mainCamera.enabled = false;
        deathUI.SetActive(true);
        playerDeath3DObject.SetActive(true);

        foreach(GameObject killer in killers) {
            KillerAI aiRef = killer.GetComponent<KillerAI>();
            //aiRef.state = Killer.State.idle;
            //aiRef.state = Killer.State.patrol; ?
        }

        int deathClipIndex = Random.Range(0, playerDeathClips.Length - 1);
        deathClipIndex = 3;
        playerDeathAnimator.SetInteger("deathClipIndex", deathClipIndex);

        yield return new WaitForSeconds(5f);
        if(deathClipIndex == 2) {
            bloodPool.SetActive(true);
        }
    }


    public void SetEndingMessage(int endingNumber) {
        endingHeader.text = "Ending " + (endingNumber + 1) + " of 3";
        endingMessage.text = endingMessages[endingNumber];
    }
}