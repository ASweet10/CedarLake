using UnityEngine;

public class StealTrigger : MonoBehaviour
{
    GameController gameController;
    [SerializeField] GameObject cashier;
    PickUpObjects pickUpObjects;
    void Start() {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        pickUpObjects = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PickUpObjects>();
    }
    void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Player"){
            if(gameController.holdingGasStationItem) {
                gameController.caughtStealing = true;
                var playerController = other.GetComponent<FirstPersonController>();
                playerController.RotateTowardsSpeaker(cashier.transform);
                pickUpObjects.DropObject();
                pickUpObjects.canDrop = false;

                var cashierTrigger = cashier.GetComponentInChildren<DialogueTrigger>();
                cashierTrigger.TriggerDialogue();
                var cashierCharacter = cashier.GetComponent<CharacterAI>();
                cashierCharacter.RotateAndStartTalking();
            }
        }
    }
    void OnTriggerStay(Collider other) {
        if(other.gameObject.tag == "Player"){
            if(gameController.holdingGasStationItem) {
                gameController.caughtStealing = true;
                var playerController = other.GetComponent<FirstPersonController>();
                playerController.RotateTowardsSpeaker(cashier.transform);
                pickUpObjects.DropObject();
                pickUpObjects.canDrop = false;

                var cashierTrigger = cashier.GetComponentInChildren<DialogueTrigger>();
                cashierTrigger.TriggerDialogue();
                var cashierCharacter = cashier.GetComponent<CharacterAI>();
                cashierCharacter.RotateAndStartTalking();
            }
        } 
    }

    void OnTriggerExit(Collider other) {
        if(other.gameObject.tag == "Player"){
            gameController.caughtStealing = false;
            pickUpObjects.canDrop = true;
        } 
    }
}