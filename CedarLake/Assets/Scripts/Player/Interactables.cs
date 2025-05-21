using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interactables : MonoBehaviour 
{
    //KeyCode escape = KeyCode.Escape; // For build
    KeyCode twoKey = KeyCode.Alpha2; // for testing


    GameController gameController;
    [SerializeField] GameObject dialogueUI;
    DialogueManager dialogueManager;
    FirstPersonController fpController;
    FirstPersonHighlights fpHighlights;
    GameObject player;
    SceneController sceneController;

    [Header ("UI Objects")]
    [SerializeField] GameObject missingUI_Matthew;
    [SerializeField] GameObject missingUI_Couple;
    [SerializeField] GameObject missingUI_Nathan;
    [SerializeField] GameObject missingUI_Maria;
    [SerializeField] GameObject missingUI_Amir;
    [SerializeField] GameObject gasStationNewspaperUI;
    [SerializeField] GameObject stateParkNewspaperUI;
    [SerializeField] GameObject UICamera;
    [SerializeField] TMP_Text interactText;


    [Header ("Gas Station Door")]
    [SerializeField] AudioSource gasStationBellAudio;
    [SerializeField] Transform gasStationSpawnpoint;
    [SerializeField] Transform gasStationParkingLotSpawnpoint;
    bool playerInGasStation;


    [Header ("Drinks")]
    [SerializeField] GameObject[] drinkOptions;
    [SerializeField] String[] drinkDescriptions;
    [SerializeField] GameObject drinkUI;
    [SerializeField] GameObject useDrinkUI;
    [SerializeField] GameObject drinkCollider;
    [SerializeField] TMP_Text drinkTitle;
    [SerializeField] TMP_Text drinkDescription;
    [SerializeField] AudioSource drinkAudio;
    [SerializeField] GameObject drinkLight;
    public int drinkIndex;


    [Header ("Pause Menu")]
    [SerializeField] GameObject pauseMenuLighterFluid;
    [SerializeField] GameObject pauseMenuZippy;
    [SerializeField] GameObject pauseMenuKeychain;
    [SerializeField] GameObject[] pauseMenuDrinks;
    [SerializeField] GameObject inventoryLighterFluid;
    [SerializeField] GameObject inventoryZippy;
    [SerializeField] GameObject inventoryMenuKeychain;
    [SerializeField] GameObject inventoryMenuDrink;


    [Header("Arcade")]
    [SerializeField] GameObject arcadeStartScreen;
    [SerializeField] GameObject arcadeLevelOne;
    [SerializeField] GameObject arcadeDeathUI;
    [SerializeField] Camera arcadeStartCamera;
    [SerializeField] Camera arcadePlayerCamera;
    [SerializeField] Camera gameCamera;
    [SerializeField] ArcadeController arcadeController;
    [SerializeField] ArcadeWolf arcadeWolfScript;
    [SerializeField] FirstPersonController firstPersonController;
    [SerializeField] MouseLook mouseLook;
    [SerializeField] AudioSource arcadeCoinSound;
    [SerializeField] AudioClip arcadeMusic;
    [SerializeField] AudioClip arcadeCoinSFX;
    [SerializeField] TMP_Text escapeToExitText;
    public bool playingArcadeGame;

    void Start () {
        playerInGasStation = false;
        playingArcadeGame = false;
        gameController = gameObject.GetComponent<GameController>();
        sceneController = GameObject.FindGameObjectWithTag("SceneController").GetComponent<SceneController>();
        player = GameObject.FindGameObjectWithTag("Player");
        dialogueManager = GameObject.FindGameObjectWithTag("Player").GetComponent<DialogueManager>();
        fpController = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
        fpHighlights = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonHighlights>();        
        drinkIndex = 0;
    }
    
    void Update() {
        HandleEscapeButtonLogic();
    }

    public void HandleGasStationDoor() {
        if(!gameController.holdingGasStationItem) { // Disable door when holding item
            StartCoroutine(FadeGasStationDoor());
        }
    }

    IEnumerator FadeGasStationDoor() {
        StartCoroutine(sceneController.FadeOut(1.5f, 99));
        yield return new WaitForSeconds(0.5f);
        gasStationBellAudio.Play();

        if(playerInGasStation) {
            player.transform.position = gasStationParkingLotSpawnpoint.position;
            playerInGasStation = false;
        } else {
            player.transform.position = gasStationSpawnpoint.position;
            playerInGasStation = true;
        }
        
        StartCoroutine(sceneController.FadeIn(1.5f));
    }

    public void ToggleDrinksUI(bool choice) {   // Drinks in gas station
        drinkUI.SetActive(choice);
        UICamera.SetActive(choice);
        drinkLight.SetActive(choice);
        drinkOptions[0].SetActive(choice);
        if(choice) {
            drinkTitle.text = drinkOptions[drinkIndex].name;
            drinkDescription.text = drinkDescriptions[drinkIndex];
        } else {
            foreach(GameObject drink in drinkOptions) {
                drink.SetActive(false);
            }
        }
    }

    public void Disable3DDrinks() {
        foreach(GameObject drink in drinkOptions) {
            drink.SetActive(false);
        }
    }

    public void ToggleUseDrinkUI(bool toggle) {
        useDrinkUI.SetActive(toggle);
    }

    public void PurchaseDrink() {
        gameController.chosenDrinkIndex = drinkIndex;
        gameController.hasDrink = true;
        TogglePauseMenuObject("drink", true);
        drinkCollider.tag = "Untagged";
        fpHighlights.ClearHighlighted();
        Disable3DDrinks();
        drinkUI.SetActive(false);
        drinkLight.SetActive(false);
        fpController.DisablePlayerMovement(false, false);
    }

    public void UseDrink() {
        StartCoroutine(fpController.HandleDrinkEffect());
        
        ToggleUseDrinkUI(false);
        gameController.hasDrink = false;
        TogglePauseMenuObject("drink", false);
        drinkAudio.Play();
    }

    public void TogglePauseMenuObject(string objectName, bool choice) {
        switch(objectName) {
            case "zippy":
                pauseMenuZippy.SetActive(choice);
                inventoryZippy.SetActive(choice);
                break;
            case "lighterFluid":
                pauseMenuLighterFluid.SetActive(choice);
                inventoryLighterFluid.SetActive(choice);
                break;
            case "keys":
                pauseMenuKeychain.SetActive(choice);
                inventoryMenuKeychain.SetActive(choice);
                break;
            case "drink":
                pauseMenuDrinks[drinkIndex].SetActive(choice);
                inventoryMenuDrink.SetActive(choice);
                break;
            default:
                break;
        }
    }
    public void ToggleMissingUI(string posterName, bool choice) {
        switch (posterName) {
            case "MissingPosterMatthew":
                missingUI_Matthew.SetActive(choice);
                break;
            case "MissingPosterCouple":
                missingUI_Couple.SetActive(choice);
                break;
            case "MissingPosterNathan":
                missingUI_Nathan.SetActive(choice);
                break;
            case "MissingPosterMaria":
                missingUI_Maria.SetActive(choice);
                break;
            case "MissingPosterAmir":
                missingUI_Amir.SetActive(choice);
                break;
            case "Newspaper_GasStation":
                gasStationNewspaperUI.SetActive(choice);
                break;
            case "Newspaper_StatePark":
                stateParkNewspaperUI.SetActive(choice);
                break;
        }
    }


    void HandleEscapeButtonLogic() {
        if(Input.GetKeyDown(twoKey)) {
            if(drinkUI.activeInHierarchy) {
                ToggleDrinksUI(false);
                fpController.DisablePlayerMovement(false, false);
                return;
            } else if(missingUI_Matthew.activeInHierarchy) {
                ToggleMissingUI("MissingPosterMatthew", false);
                fpController.DisablePlayerMovement(false, false);
                return;
            } else if(missingUI_Couple.activeInHierarchy) {
                ToggleMissingUI("MissingPosterCouple", false);
                fpController.DisablePlayerMovement(false, false);
                return;
            } else if(missingUI_Nathan.activeInHierarchy) {
                ToggleMissingUI("MissingPosterNathan", false);
                fpController.DisablePlayerMovement(false, false);
                return;
            } else if(missingUI_Maria.activeInHierarchy) {
                ToggleMissingUI("MissingPosterMaria", false);
                fpController.DisablePlayerMovement(false, false);
                return;
            } else if(missingUI_Amir.activeInHierarchy) {
                ToggleMissingUI("MissingPosterAmir", false);
                fpController.DisablePlayerMovement(false, false);
                return;
            } else if(gasStationNewspaperUI.activeInHierarchy) {
                ToggleMissingUI("Newspaper_GasStation", false);
                fpController.DisablePlayerMovement(false, false);
                return;
            } else if(stateParkNewspaperUI.activeInHierarchy) {
                ToggleMissingUI("Newspaper_StatePark", false);
                fpController.DisablePlayerMovement(false, false);
                return;
            } else if(dialogueUI.activeInHierarchy) {
                dialogueManager.DialogueStop();
            } else if(playingArcadeGame) {
                StartCoroutine(ToggleArcade(false));
                playingArcadeGame = false;
            } else {
                if(gameController.gamePaused) {
                    gameController.ResumeGame();
                    Debug.Log("resume");
                }
                else {
                    gameController.PauseGame();
                }
            }
        }
    }

    public void HandlePreviousDrinkButton() {
        drinkOptions[drinkIndex].SetActive(false);
        if(drinkIndex == 0) {
            drinkIndex = drinkOptions.Length - 1;
        } else {
            drinkIndex --;
        }
        drinkOptions[drinkIndex].SetActive(true);
        drinkTitle.text = drinkOptions[drinkIndex].name;
        drinkDescription.text = drinkDescriptions[drinkIndex];
    }
    public void HandleNextDrinkButton() {
        drinkOptions[drinkIndex].SetActive(false);
        if(drinkIndex == drinkOptions.Length - 1) {
            drinkIndex = 0;
        } else {
            drinkIndex ++;
        }
        drinkOptions[drinkIndex].SetActive(true);
        drinkTitle.text = drinkOptions[drinkIndex].name;
        drinkDescription.text = drinkDescriptions[drinkIndex];
    }
    
    public IEnumerator ToggleArcade(bool playingGame) {
        if(playingGame) {
            arcadeStartCamera.enabled = true;
            gameCamera.enabled = false;
            arcadeController.enabled = true;
            firstPersonController.enabled = false;
            mouseLook.enabled = false;
            interactText.text = "";

            arcadeCoinSound.Play();
            yield return new WaitForSeconds(1.5f);
            arcadeStartScreen.SetActive(false);
            arcadeLevelOne.SetActive(true);
            arcadePlayerCamera.enabled = true;
            arcadeStartCamera.enabled = false;

            arcadeController.ResetArcadePlayerPosition();
            arcadeWolfScript.ResetWolfPosition();

            arcadeController.CanMove = true;
            arcadeCoinSound.clip = arcadeMusic;
            arcadeCoinSound.Play();
            arcadeCoinSound.loop = true;

        } else {
            gameCamera.enabled = true;
            arcadeStartCamera.enabled = false;
            arcadePlayerCamera.enabled = false;

            arcadeController.enabled = false;
            firstPersonController.enabled = true;
            mouseLook.enabled = true;
            arcadeCoinSound.Stop();
            arcadeCoinSound.clip = arcadeCoinSFX; // reset for next game

            if(arcadeLevelOne.activeInHierarchy) {
                arcadeLevelOne.SetActive(false);
            }
            if(arcadeDeathUI.activeInHierarchy) {
                arcadeDeathUI.SetActive(false);
            }
            arcadeStartScreen.SetActive(true);
            escapeToExitText.enabled = false;
        }
    }
}