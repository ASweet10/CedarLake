using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Rendering; 
using UnityEngine.Rendering.Universal;

public class GameController : MonoBehaviour
{
    [SerializeField] GameObject playerRef;
    [SerializeField] GameObject pauseMenuUI;
    [SerializeField] GameObject quitGameOptionUI;
    [SerializeField] GameObject leaveEarlyUI;
    [SerializeField] GameObject mainMenuChoiceUI;
    [SerializeField] Interactables interactables;
    [SerializeField] SceneController sceneController;
    [SerializeField] FirstPersonController fpController;
    [SerializeField] KillerAI killerAI;
    CutsceneManager cutsceneManager;


    [Header("Objectives")]
    [SerializeField] TMP_Text popupText;
    [SerializeField] TMP_Text objectivePopupText;
    [SerializeField] TMP_Text objectiveTextInPauseMenu;
    [SerializeField] string[] gameObjectives;
    public int currentObjective = 4;


    public bool holdingGasStationItem = false;
    public bool hasPurchasedGas = false;
    public bool hasZippo = false;
    public bool hasLighterFluid = false;
    public bool hasCarKeys = false;
    public bool fireStarted = false;
    public bool caughtStealing = false;
    public bool hunterWarningComplete = false;
    public bool gamePaused = false;
    public bool hasFirewood = false;

    public bool hasDrink = false;
    public int chosenDrinkIndex = 0;
    public string currentSpeaker;
    


    [Header("Player Death & Checkpoints")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera deathCamera;
    [SerializeField] GameObject playerDeath3DObject;
    [SerializeField] AnimationClip[] playerDeathClips;
    [SerializeField] Animator playerDeathAnimator;
    [SerializeField] GameObject deathUI;
    [SerializeField] Transform[] restartPositions;
    [SerializeField] GameObject[] killers;
    int currentCheckpoint = 0;



    [Header ("Main Menu")]
    [SerializeField] GameObject mainMenuUI;
    [SerializeField] GameObject optionsMenuUI;
    [SerializeField] GameObject[] expositionUIObjects;



    [Header("Endings")]
    [SerializeField] GameObject endingUI;
    [SerializeField] GameObject threeAMUI;
    [SerializeField] TMP_Text endingHeader;
    [SerializeField] TMP_Text endingMessage;
    [SerializeField] AudioSource endingCarAudio;



    [Header("Events")]
    FirstPersonHighlights fpHighlights;
    [SerializeField] AudioSource itemPickupAudio;
    [SerializeField] GameObject hunter;
    [SerializeField] GameObject zippo;
    [SerializeField] GameObject lighterFluid;



    [Header ("Campfire")]
    [SerializeField] GameObject[] firewood;
    [SerializeField] GameObject woodBundle;
    [SerializeField] GameObject fireSmall;
    [SerializeField] GameObject fireMediumSmoke;
    [SerializeField] GameObject fireBigSmoke;
    [SerializeField] GameObject campfire;
    [SerializeField] GameObject campfireCollider;
    [SerializeField] GameObject campfireTransparent;
    [SerializeField] Transform campfirePosition;
    [SerializeField] Animator fireAnimator;
    float smallFireTime = 5f;
    float mediumFireTime = 10f;



    [Header("Day/Night")]
    [SerializeField] Material nightSkyboxMat;
    [SerializeField] GameObject directionalLightDay;
    [SerializeField] GameObject directionalLightNight;
    [SerializeField] GameObject davidRef;
    [SerializeField] GameObject deadBody;
    [SerializeField] GameObject[] tentObjects;
    [SerializeField] GameObject playerTent;
    [SerializeField] GameObject axeInStump;
    Volume volume;
    ColorAdjustments colorAdjustments;    

    
    void Start() {
        cutsceneManager = gameObject.GetComponent<CutsceneManager>();
        fpHighlights = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonHighlights>();
        volume = gameObject.GetComponent<Volume>();
        volume.profile.TryGet(out colorAdjustments);

        foreach(GameObject killer in killers) {
            KillerAI aiRef = killer.GetComponent<KillerAI>();
            aiRef.enabled = true;
        }
        currentObjective = 4;
        //currentCheckpoint = 0;

        if(SceneManager.GetActiveScene().buildIndex != 1) {  // If main menu / ending
            fpController.DisablePlayerMovement(true, true);
        }
        else {
            interactables.enabled = true;
        }
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Alpha3)) {
            StartCoroutine(TransitionToNighttime());
        }
    }

    public void ReplayFromDeath() {
        playerRef.transform.position = restartPositions[currentCheckpoint].position;
        playerRef.transform.rotation = restartPositions[currentCheckpoint].rotation;

        fpController.DisablePlayerMovement(false, false);
        fpController.EnableCollider(true);
        deathCamera.enabled = false;
        mainCamera.enabled = true;
        deathUI.SetActive(false);
        playerDeath3DObject.SetActive(false);
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
    public void SetEndingMessage(int endingNumber) {
        endingHeader.text = "Ending " + (endingNumber + 1) + " of 3";

    }


    /* Events */
    public void HandleBuildFire() {
        woodBundle.SetActive(false);
        campfireTransparent.SetActive(false);
        campfire.SetActive(true);
        campfire.tag = "Start Fire";
        hasFirewood = false;

        zippo.tag = "Zippy";
        zippo.GetComponent<BoxCollider>().enabled = true;
        lighterFluid.tag = "Lighter Fluid";
        lighterFluid.GetComponent<BoxCollider>().enabled = true;

        StartCoroutine(HandleNextObjective());
    }

    public void HandleCollectKeyItem(string item) {
        switch(item) {
            case "Zippy":
                hasZippo = true;
                interactables.TogglePauseMenuObject("zippy", true);
                if(hasLighterFluid) {
                    StartCoroutine(HandleNextObjective());
                }
                break;
            case "LighterFluid":
                hasLighterFluid = true;
                interactables.TogglePauseMenuObject("lighterFluid", true);
                if(hasZippo) {
                    StartCoroutine(HandleNextObjective());
                }
                break;
            case "CarKeys":
                hasCarKeys = true;
                interactables.TogglePauseMenuObject("keys", true);
                StartCoroutine(HandleNextObjective());
                // Change player car tag? Can now leave?
                break;
            case "Firewood":
                hasFirewood = true;
                StartCoroutine(HandleNextObjective());
                campfireTransparent.SetActive(true); // Enable transparent campfire for interaction
                foreach(GameObject wood in firewood) {
                    wood.SetActive(false);
                }
                woodBundle.SetActive(true);

                foreach(GameObject tent in tentObjects) {
                    tent.SetActive(true);
                }
                break;
            }

        itemPickupAudio.Play();
    }

    public void StartCampFire() {
        fpHighlights.ClearHighlighted();

        campfire.tag = "Untagged";
        campfireCollider.tag = "Untagged";
        playerTent.tag = "Go To Sleep";

        interactables.TogglePauseMenuObject("zippy", false);
        interactables.TogglePauseMenuObject("lighterFluid", false);

        StartCoroutine(HandleFireAnimation());
        fireStarted = true;

        StartCoroutine(HandleNextObjective());
        hunter.SetActive(true);
    }

    IEnumerator HandleFireAnimation() {
        fireAnimator.Play("StartFire");
        yield return new WaitForSeconds(7f);
        var smallFire = Instantiate(fireSmall, campfirePosition.position, Quaternion.identity);
        yield return new WaitForSecondsRealtime(smallFireTime);
        Destroy(smallFire);
        var mediumFire = Instantiate(fireMediumSmoke, campfirePosition.position, Quaternion.identity);
        yield return new WaitForSecondsRealtime(mediumFireTime);
        Destroy(mediumFire);
        Instantiate(fireBigSmoke, campfirePosition.position, Quaternion.identity);
    }

    public IEnumerator TransitionToNighttime() {
        cutsceneManager.ToggleCutscene("Campfire", true);
        fpController.DisablePlayerMovement(true, false);
        tentObjects[0].SetActive(false); // disable tent flap (David)
        axeInStump.SetActive(false);

        yield return new WaitForSeconds(8f);
        cutsceneManager.ToggleCutscene("Campfire", false);

        fpController.DisablePlayerMovement(false, false);
        RenderSettings.skybox = nightSkyboxMat;
        directionalLightDay.SetActive(false);
        directionalLightNight.SetActive(true);
        colorAdjustments.hueShift.value = 2f;
        
        yield return new WaitForSeconds(2f);
    }
    public void HandleLeaveTent() {
        // fade out 1-2 sec, play zip sfx, fade in 1-2 sec
        // Disable all tent flaps
        tentObjects[0].SetActive(false);
        tentObjects[1].SetActive(false);
        tentObjects[1].SetActive(false);
        playerTent.tag = "Untagged";
        //blood vfx on ground near tent? or does that clue player in too soon? maybe a trail starting in woods nearby

        davidRef.SetActive(false);
        // blood vfx on ground near body?
        deadBody.SetActive(true);
        // enable killers?
        // enable "hider" killer in one of 10? 12? randomized spots?
    }

    public void StartDriveToParkCutscene() {

    }

    public void HandlePlayerDeath() {
        fpController.DisablePlayerMovement(true, true);
        fpController.EnableCollider(false);
        deathCamera.enabled = true;
        mainCamera.enabled = false;
        deathUI.SetActive(true);

        playerDeath3DObject.SetActive(true);
        int deathClipIndex = Random.Range(0, playerDeathClips.Length - 1);
        playerDeathAnimator.SetInteger("deathClipIndex", deathClipIndex);
    }

    public IEnumerator HandleEndGame(int endingNumber) {
        sceneController.FadeOut(2, 99);
        endingCarAudio.Play();
        yield return new WaitForSeconds(2.5f);

        fpController.DisablePlayerMovement(true, true);
        fpController.EnableCollider(false);
        endingUI.SetActive(true);

        switch (endingNumber) {
            case 1:
                endingHeader.text = "I";
                endingMessage.text = "You thought about it and realized this is probably a bad idea. You'd rather be at home snuggled up in a blanket.";
                break;
            case 2:
                endingHeader.text = "II";
                endingMessage.text = "You managed to escape and immediately alerted the authorities. You tell them that you weren't able to save your friends.";
                break;
            case 3:
                endingHeader.text = "III";
                endingMessage.text = "Most would focus on saving their own skin and you went back. You are truly a good friend.";
                break;
        }
    }

    /**** Menus ****/
    public void ToggleLeaveEarlyUI(bool choice) {
        leaveEarlyUI.SetActive(choice);
    }
    public void ConfirmLeaveEarly() {
        StartCoroutine(HandleEndGame(1));
    }
    public void ToggleQuitGameUI(bool choice) {
        quitGameOptionUI.SetActive(choice);
    }
    public void ConfirmQuitGame() {
        Application.Quit();
    }
    public void ToggleMainMenuChoice(bool choice) {
        mainMenuChoiceUI.SetActive(choice);
    }
    public void ConfirmMainMenu() {
        SceneManager.LoadScene(0);
    }

    public void ToggleOptionsMenu(bool toggle) {
        mainMenuUI.SetActive(!toggle);
        optionsMenuUI.SetActive(toggle);
    }
    public void PlayGameButton() {
        StartCoroutine(sceneController.PanCameraOnPlay());
        //StartCoroutine(sceneController.FadeOut(2, 1));
    }

    public void ResumeGame() {
        objectiveTextInPauseMenu.enabled = false;
        pauseMenuUI.SetActive(false);
        popupText.enabled = true;
        AudioListener.volume = 1f;
        fpController.DisablePlayerMovement(false, false);
        killerAI.DisableKillerMovement(true);
    }
    public void PauseGame() {
        pauseMenuUI.SetActive(true);
        popupText.enabled = false;
        objectiveTextInPauseMenu.enabled = true;
        objectiveTextInPauseMenu.text = gameObjectives[currentObjective];
        AudioListener.volume = 0.3f;
        fpController.DisablePlayerMovement(true, true);
        killerAI.DisableKillerMovement(false);
    }
}