using System.Collections;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    CharacterController controller;
    Transform tf;
    [SerializeField] Camera mainCamera;
    [SerializeField] MouseLook mouseLook;
    MovementSFX movementSFX;

    [SerializeField] FlashlightToggle flashlight;
    FirstPersonHighlights fpHighlights;

    bool shouldCrouch => !duringCrouchAnimation && controller.isGrounded && Input.GetKeyDown(KeyCode.C);


    [Header("Movement")]
    [SerializeField] float walkSpeed = 10f;
    [SerializeField] float sprintSpeed = 20f;
    [SerializeField] float maxStamina = 15f; // [SerializeField, Range(1, 20)]
    [SerializeField] AudioSource windedAudio;
    KeyCode sprintKey = KeyCode.LeftShift;
    Vector2 currentInput;
    Vector3 currentMovement;
    public bool isSprinting => canSprint && Input.GetKey(sprintKey);
    public bool isMoving => Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
    float currentStamina;
    bool canSprint = true;




    [Header("Crouch")]
    [SerializeField] float crouchHeight = 0.5f;
    [SerializeField] float standHeight = 2f;
    [SerializeField] Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] Vector3 standCenter = new Vector3(0, 0, 0);
    [SerializeField] float timeToCrouch = 0.25f;
    [SerializeField, Range(1, 5)] float crouchSpeed = 2.5f;
    public bool isCrouching;
    bool duringCrouchAnimation;
    bool canCrouch = true;


    [Header("Headbob")]
    [SerializeField] float walkBobSpeed = 14f;
    [SerializeField] float walkBobAmount = 0.05f;
    [SerializeField] float crouchBobSpeed = 8f;
    [SerializeField] float crouchBobAmount = 0.025f;
    [SerializeField] float sprintBobSpeed = 18f;
    [SerializeField] float sprintBobAmount = 0.1f;
    float defaultYPosition = 0;
    float timer;


    GameController gameController;
    public bool canMove = true;

    void Awake() {
        fpHighlights = gameObject.GetComponent<FirstPersonHighlights>();
        controller = gameObject.GetComponent<CharacterController>();
        movementSFX = gameObject.GetComponent<MovementSFX>();
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        defaultYPosition = mainCamera.transform.localPosition.y; // Return camera to default position when not moving
        tf = gameObject.GetComponent<Transform>();
    }

    void Start() {  
        DisablePlayerMovement(false, false);
        currentStamina = maxStamina;
    }

    void Update()
    {
        if (canMove)
        {
            HandleMovementInput();
            if (canCrouch)
            {
                AttemptToCrouch();
            }
            HandleHeadbobEffect();
            HandleStamina();
            movementSFX.HandleMovementSFX();
            ApplyFinalMovement();
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            gameController.chosenDrinkIndex = 0;
            StartCoroutine(HandleDrinkEffect());
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            gameController.chosenDrinkIndex = 1;
            StartCoroutine(HandleDrinkEffect());
        }
        if (Input.GetKeyDown(KeyCode.Alpha9)) {
            gameController.chosenDrinkIndex = 2;
            StartCoroutine(HandleDrinkEffect());
        }
    }



    public IEnumerator HandleDrinkEffect() {
        switch(gameController.chosenDrinkIndex) {
            case 0:
                sprintSpeed += 10;
                yield return new WaitForSeconds(8f);
                sprintSpeed -= 10;
                break;
            case 1:
                maxStamina = 9999f;
                currentStamina = maxStamina;
                yield return new WaitForSeconds(15f);
                maxStamina = 15f;
                currentStamina = maxStamina;
                break;
            case 2:
                sprintSpeed += 5;
                yield return new WaitForSeconds(15f);
                sprintSpeed -= 5;
                break;
        }
        yield return null;
    }

    public void DisablePlayerMovement(bool disableMovement, bool showCursor) {
        if (disableMovement) {
            canMove = false;
            fpHighlights.CanInteract = false;
            mouseLook.CanRotateMouse = false;
            currentMovement = Vector3.zero;
            flashlight.ToggleFlashlightStatus(false);
            movementSFX.enabled = false;
        }
        else {
            canMove = true;
            fpHighlights.CanInteract = true;
            mouseLook.CanRotateMouse = true;
            flashlight.ToggleFlashlightStatus(true);
            movementSFX.enabled = true;
        }
        
        if(showCursor) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        } else {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
    }

    public void EnableCollider(bool choice) {
        controller.detectCollisions = choice;
    }

    public void RotateTowardsSpeaker(Transform target) {
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.LookAt(targetPosition);
    }
     public void AttemptToCrouch() {
        if(shouldCrouch) {
            StartCoroutine(CrouchOrStand());
        }
    }
    private IEnumerator CrouchOrStand() {
        // If you try to stand up and hit anything 1 unit above, cancel and remain crouched
        if (isCrouching && Physics.Raycast(mainCamera.transform.position, Vector3.up, 1f)) {
            yield break;
        }
        duringCrouchAnimation = true;
        float timeElapsed = 0f;

        // Change height
        float targetHeight = isCrouching ? standHeight : crouchHeight;
        float currentHeight = controller.height;
        // Change center so you don't fall through floor
        Vector3 targetCenter = isCrouching ? standCenter : crouchCenter;
        Vector3 currentCenter = controller.center;

        while(timeElapsed < timeToCrouch) {
            controller.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        controller.height = targetHeight;
        controller.center = targetCenter;
        isCrouching = !isCrouching;
        duringCrouchAnimation = false;
    }

    void HandleMovementInput() {
        currentInput.x = Input.GetAxis("Vertical") * (isCrouching ? crouchSpeed : isSprinting ? sprintSpeed :  walkSpeed);
        currentInput.y = Input.GetAxis("Horizontal") * (isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed);

        float currentMovementY = currentMovement.y;
        currentMovement = (tf.forward * currentInput.x) + (transform.right * currentInput.y);
        currentMovement.y = currentMovementY;
    }

    void ApplyFinalMovement() {
        if(!controller.isGrounded){
            currentMovement.y -= 9.8f * Time.deltaTime; // Apply gravity
            if(controller.velocity.y < -1 && controller.isGrounded){  //Landing frame; reset y value to 0
                currentMovement.y = 0;
            }
        }
        /*
        if (IsSliding)
        {
            currentMovement += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }
        */
        controller.Move(currentMovement * Time.deltaTime);
    }
    
    void HandleHeadbobEffect() {
        if(!controller.isGrounded) {
            return;
        }
        if(Mathf.Abs(currentMovement.x) > 0.1f || Mathf.Abs(currentMovement.z) > 0.1f) {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : isSprinting ? sprintBobSpeed : walkBobSpeed);
             // return Sin of angle; between -1 and 1
             //  Multiply this value by amount depending on current movement
            mainCamera.transform.localPosition = new Vector3(
                mainCamera.transform.localPosition.x,
                defaultYPosition + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : isSprinting ? sprintBobAmount : walkBobAmount),
                mainCamera.transform.localPosition.z
            );
        }
    }
    void HandleStamina() {
        if(currentStamina < 0) {
            currentStamina = 0;
            windedAudio.Play();
            canSprint = false;
        }

        if(isSprinting) {
            currentStamina -= 1f * Time.deltaTime; // Sprinting
        } else {
            if(canSprint == false) {
                if(currentStamina < maxStamina) { // Not sprinting, regenerate stamina
                    currentStamina += 1f * Time.deltaTime;
                } else {
                    currentStamina = maxStamina;
                    canSprint = true;
                }
            }
        }
    }


    /*
    [Header("Jump")]
    [SerializeField] float jumpForce = 10f;
    KeyCode jumpKey = KeyCode.Space;
    bool canJump = true;
    bool shouldJump => controller.isGrounded && Input.GetKeyDown(jumpKey);
    
    void HandleJump() {
        if(shouldJump) {
            currentMovement.y = jumpForce;
        }
    }
    // Sliding
    Vector3 hitPointNormal; // Angle of floor
    float slopeSpeed = 8f;

    bool IsSliding { 
        get {
            if(controller.isGrounded && Physics.Raycast(tf.position, Vector3.down, out RaycastHit slopeHit, 2f)) {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > controller.slopeLimit;
            } else {
                return false;
            }
        }
    }
    */
}