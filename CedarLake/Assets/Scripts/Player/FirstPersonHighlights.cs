using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonHighlights : MonoBehaviour
{
    KeyCode interactKey = KeyCode.E;
    GameController gameController;
    Interactables interactables;
    Cutscenes cutscenes;
    [SerializeField] PickUpObjects pickupObjects;
    FirstPersonController fpController;
    [SerializeField] ArcadeController arcadeController;

    [SerializeField] Camera mainCamera;
    public bool CanInteract;
    
    [Header("Highlights")]
    GameObject lastHighlightedObject;
    [SerializeField] Image cursorImage;
    [SerializeField] TMP_Text interactText; // Text displayed on hover

    [Header("Interact Texts")]
    [SerializeField] string trashString = "Ugh it smells awful...";
    [SerializeField] string myCarString = "She's an old piece of shit but reliable";
    [SerializeField] string davidCarString = "David's new wheels. He can't stop bragging about it";
    [SerializeField] string needsZippoAndLighterFluidString = "I'm gonna need lighter fluid and a lighter";
    [SerializeField] string needsZippoString = "I still need a source of fire...";
    [SerializeField] string needsLighterFluidString = "I still need lighter fluid...";
    
    void Awake() {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>(); 
        interactables = GameObject.FindGameObjectWithTag("GameController").GetComponent<Interactables>();
        cutscenes = GameObject.FindGameObjectWithTag("GameController").GetComponent<Cutscenes>();
        fpController = gameObject.GetComponent<FirstPersonController>();
    }

    void Update() {
        if(CanInteract) { HandleInteraction(); }
    }
    void DetermineInteractionType(GameObject hitObj) {
        switch(hitObj.GetComponent<Collider>().gameObject.tag) {
            case "Door":
                interactables.HandleGasStationDoor();
                break;
            case "MissingPoster":
                fpController.DisablePlayerMovement(true, false);
                interactables.ToggleMissingUI(hitObj.gameObject.name, true);
                ClearHighlighted();
                break;
            case "Newspaper":
                fpController.DisablePlayerMovement(true, false);
                interactables.ToggleMissingUI(hitObj.gameObject.name, true);
                ClearHighlighted();
                break;
            case "Drinks":
                interactables.ToggleDrinksUI(true);
                fpController.DisablePlayerMovement(true, true);
                break;
            case "Pick Up":
                pickupObjects.HandlePickUpObject(hitObj);
                break;
            case "Trash":
                StartCoroutine(gameController.DisplayPopupMessage(trashString));
                break;
            case "My Car":
                switch(gameController.currentObjective) {
                    case 2:
                        gameController.StartDriveToParkCutscene(); // If checkpoint is "drive to park" after gas
                        break;
                    case 5:
                        gameController.ToggleLeaveEarlyUI(true); // If player can leave (ending 1), open UI option
                        break;
                    case 7:
                        // Player tries to leave at night (tires slashed)
                        break;
                    case 12:
                        StartCoroutine(gameController.HandleEndGame(2)); // Player leaves alone (ending 2)
                        break;
                    case 13:
                        StartCoroutine(gameController.HandleEndGame(3)); // Player leaves with friend nearby (ending 3)
                        break;
                    default:
                        StartCoroutine(gameController.DisplayPopupMessage(myCarString));
                        break;
                }
                break;
            case "David's Car":
                StartCoroutine(gameController.DisplayPopupMessage(davidCarString));
                break;
            case "Arcade Game":
                arcadeController.playingArcadeGame = true;
                StartCoroutine(arcadeController.HandleEnableArcade(true));
                interactText.text = "";
                fpController.DisablePlayerMovement(true, false);
                break;
            case "Firewood":
                gameController.HandleCollectKeyItem("Firewood");
                hitObj.SetActive(false);
                break;
            case "Zippy":
                gameController.HandleCollectKeyItem("Zippy");
                hitObj.SetActive(false);
                break;
            case "Lighter Fluid":
                gameController.HandleCollectKeyItem("LighterFluid");
                hitObj.SetActive(false);
                break;
            case "Start Fire":
                if(!gameController.hasLighterFluid) {
                    if(!gameController.hasZippo) {
                        StartCoroutine(gameController.DisplayPopupMessage(needsZippoAndLighterFluidString));
                    } else {
                        StartCoroutine(gameController.DisplayPopupMessage(needsLighterFluidString));
                    }
                } else if (!gameController.hasZippo) {
                    StartCoroutine(gameController.DisplayPopupMessage(needsZippoString));
                } else {
                    gameController.StartCampFire();
                }
                break;
            case "Build Campfire":
                if(gameController.hasFirewood) {
                    gameController.HandleBuildFire();
                }
                break;
            case "Go To Sleep":
                StartCoroutine(gameController.TransitionToNighttime());
                break;
            case "Leave Tent":
                gameController.HandleLeaveTent();
                break;
            case "Head To Park":
                StartCoroutine(cutscenes.HandleDriveToParkCutscene());
                break;
            case "HiddenItem":
                break;
            case "Car Keys":
                gameController.HandleCollectKeyItem("CarKeys");
                hitObj.SetActive(false);
                break;
            case "Cashier":
                var cashierTrigger = hitObj.GetComponentInChildren<DialogueTrigger>();
                cashierTrigger.TriggerDialogue();
                var cashierCharacter = hitObj.GetComponent<CharacterAI>();
                cashierCharacter.RotateAndStartTalking();
                gameController.currentSpeaker = "Cashier";
                break;
            case "AJ":
                var ajTrigger = hitObj.GetComponentInChildren<DialogueTrigger>();
                ajTrigger.TriggerDialogue();
                var ajCharacter = hitObj.GetComponent<CharacterAI>();
                ajCharacter.RotateAndStartTalking();
                gameController.currentSpeaker = "AJ";
                break;
            case "David":
                var davidTrigger = hitObj.GetComponentInChildren<DialogueTrigger>();
                davidTrigger.TriggerDialogue();
                var davidCharacter = hitObj.GetComponent<CharacterAI>();
                davidCharacter.RotateAndStartTalking();
                gameController.currentSpeaker = "David";
                break;
            case "Hunter":
                var hunterTrigger = hitObj.GetComponentInChildren<DialogueTrigger>();
                hunterTrigger.TriggerDialogue();
                var hunterCharacter = hitObj.GetComponent<CharacterAI>();
                hunterCharacter.RotateAndStartTalking();
                gameController.currentSpeaker = "Hunter";
                break;
            default:
                ClearHighlighted();
                break;
        }
    }
    void HandleInteraction() {
        float rayDistance = 50f;
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // Ray from center of the viewport
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, rayDistance)) {
            GameObject hitObj = rayHit.collider.gameObject;  // Get object that was hit
            if(Vector3.Distance(gameObject.transform.position, hitObj.transform.position) < 6f) {
                HighlightObject(hitObj, true);
                if(Input.GetKeyDown(interactKey)) {
                    DetermineInteractionType(hitObj);
                }
            } else {
                ClearHighlighted();
                if(hitObj.tag != "Untagged") {
                    var outline = hitObj.GetComponent<Outline>();
                    if(outline != null) {
                        outline.enabled = false;
                    }
                }
            }
        }
    }
    public void ClearHighlighted() {
        if (lastHighlightedObject != null) {
            //lastHighlightedObject.GetComponent<MeshRenderer>().material = originalMat;
            lastHighlightedObject = null;
            cursorImage.enabled = true;
            interactText.enabled = false;
        }
    }
    void HighlightObject(GameObject hitObj, bool uiEnabled) {
        if (lastHighlightedObject != hitObj) {
            ClearHighlighted();
            lastHighlightedObject = hitObj;
            var outline = hitObj.GetComponentInChildren<Outline>();
            if(outline != null) {
                outline.enabled = true;
            }

            if(uiEnabled) {
                interactText.enabled = true;
                if(hitObj.tag == "Untagged" || hitObj.tag == "Player" || hitObj.tag == "Tile") {
                    cursorImage.enabled = false;
                    interactText.text = "";
                } else {
                    cursorImage.enabled = false;
                    interactText.text = hitObj.tag;
                }
            }
        }
    }
}