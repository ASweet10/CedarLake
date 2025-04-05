using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Rendering; 
using UnityEngine.Rendering.Universal;

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
    public int currentObjective = 4;


    public bool holdingGasStationItem = false;
    public bool hasPurchasedGas = false;
    public bool playerHasReadCarNote = false;
    public bool playerNeedsFirewood = true;
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
    int currentCheckpoint = 0;



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

    [Header ("Events")]
    FirstPersonHighlights fpHighlights;
    [SerializeField] AudioSource itemPickupAudio;
    [SerializeField] GameObject hunter;
    [SerializeField] GameObject zippo;
    [SerializeField] GameObject lighterFluid;



    [Header ("Campfire")]
    [SerializeField] GameObject[] firewood;
    int firewoodCollected;
    int firewoodMax = 2;  //  change to 6 for game, 2 for testing
    [SerializeField] GameObject fireSmall;
    [SerializeField] GameObject fireMediumSmoke;
    [SerializeField] GameObject fireBigSmoke;
    [SerializeField] GameObject campfire;
    [SerializeField] GameObject campfireCollider;
    [SerializeField] GameObject campfireTransparent;
    [SerializeField] Transform campfirePosition;
    float smallFireTime = 5f;
    float mediumFireTime = 10f;

    
    [Header("Day/Night")]
    [SerializeField] Material nightSkyboxMat;
    [SerializeField] GameObject directionalLightDay;
    [SerializeField] GameObject directionalLightNight;
    [SerializeField] GameObject davidRef;
    [SerializeField] GameObject[] tents;
    Volume volume;
    ColorAdjustments colorAdjustments;
    Bloom bloom;
    

    
    void Start() {
        fpHighlights = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonHighlights>();
        volume = gameObject.GetComponent<Volume>();
        volume.profile.TryGet(out colorAdjustments);
        volume.profile.TryGet(out bloom);

        //currentObjective = 0;
        //currentCheckpoint = 0;
        firewoodCollected = 0;

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
        if(Input.GetKeyDown(KeyCode.Alpha3)) {
            TransitionToNighttime();
        }
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



    /* Events */
    public void HandleBuildFire() {
        campfireTransparent.SetActive(false);
        campfire.SetActive(true);
        campfire.tag = "Start Fire";

        zippo.tag = "Zippo";
        zippo.GetComponent<BoxCollider>().enabled = true;
        lighterFluid.tag = "Lighter Fluid";
        lighterFluid.GetComponent<BoxCollider>().enabled = true;

        StartCoroutine(HandleNextObjective());
    }

    public void HandleCollectKeyItem(string item) {
        switch(item) {
            case "Zippy":
                hasZippo = true;
                interactables.TogglePauseMenuObject("zippo", true);
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
                firewoodCollected ++;

                if(firewoodCollected < firewoodMax) {
                    StartCoroutine(DisplayPopupMessage("Collect firewood (" + firewoodCollected + " of " + firewoodMax + ")"));
                } else {
                    StartCoroutine(DisplayPopupMessage("Time to build a fire"));
                    playerNeedsFirewood = false;
                    StartCoroutine(HandleNextObjective());

                    campfireTransparent.SetActive(true); // Enable transparent campfire for interaction

                    foreach(GameObject wood in firewood) {
                        wood.tag = "Untagged";
                        var woodCollider = wood.GetComponent<BoxCollider>();
                        woodCollider.enabled = false;
                    }
                    foreach(GameObject tent in tents) {
                        tent.SetActive(true);
                    }
                }

                break;
            }

        itemPickupAudio.Play();
    }

        public IEnumerator StartCampFire() {
        fpHighlights.ClearHighlighted();

        campfire.tag = "Untagged";
        campfireCollider.tag = "Untagged";

        interactables.TogglePauseMenuObject("zippo", false);
        interactables.TogglePauseMenuObject("lighterFluid", false);

        // Animation of lighter fluid pouring onto fire
        // Animation of zippo starting fire? 
        //   or match thrown onto fire? (change from zippo -> match?)
        var smallFire = Instantiate(fireSmall, campfirePosition.position, Quaternion.identity);
        yield return new WaitForSecondsRealtime(smallFireTime);
        Destroy(smallFire);
        var mediumFire = Instantiate(fireMediumSmoke, campfirePosition.position, Quaternion.identity);
        yield return new WaitForSecondsRealtime(mediumFireTime);
        Destroy(mediumFire);
        Instantiate(fireBigSmoke, campfirePosition.position, Quaternion.identity);
        fireStarted = true;

        StartCoroutine(HandleNextObjective());
        
        hunter.SetActive(true);
    }

    public void TransitionToNighttime() {
        RenderSettings.skybox = nightSkyboxMat;
        directionalLightDay.SetActive(false);
        directionalLightNight.SetActive(true);

        colorAdjustments.hueShift.value = 2f;
        /* Disable david's tent flap, not the other one
         player has to open theirs with e, tent sound & fade out/in

        foreach(GameObject tentFlap in tentFlaps) {
            tentFlap.SetActive(false);
        }
        */

        //davidRef.tag = "Untagged"; // Set tag to null; disable dialog
        //var davidCharacter = davidRef.GetComponent<CharacterAI>();
        //davidCharacter.StateRef = CharacterAI.State.dead;
    }

    public void StartDriveToParkCutscene() {

    }
}