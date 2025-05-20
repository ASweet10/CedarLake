using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.UI;

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

    [Header("Movement")]
    [SerializeField] float walkSpeed = 10f;
    [SerializeField] float sprintSpeed = 20f;
    KeyCode sprintKey = KeyCode.LeftShift;
    Vector2 currentInput;
    Vector3 currentMovement;
    public bool isSprinting => canSprint && Input.GetKey(sprintKey);
    public bool isMoving => Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;


    [Header("Health")]
    [SerializeField] AudioSource damageAudio;
    [SerializeField] AudioSource heartbeatAudio;
    [SerializeField] RawImage bloodUI;
    [SerializeField] GameObject redBG;
    [SerializeField] Volume volume;
    Vignette vignette;
    ColorAdjustments colorAdjustments;
    float maxHealth = 3;
    float currentHealth;
    [SerializeField] float maxVignetteIntensity = 0.7f;
    [SerializeField] float maxSaturation = 75f;
    bool canTakeDamage = true;
    bool isRegenerating = false;



    [Header("Stamina")]
    [SerializeField] float maxStamina = 15f; // [SerializeField, Range(1, 20)]
    [SerializeField] AudioSource windedAudio;
    float currentStamina;
    bool canSprint = true;


    // Sliding
    Vector3 hitPointNormal; // Angle of floor
    float slopeSpeed = 8f;


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

        volume = GameObject.FindGameObjectWithTag("GameController").GetComponent<Volume>();
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out colorAdjustments);
    }

    void Start() {  
        DisablePlayerMovement(false, false);
        currentStamina = maxStamina;
        currentHealth = maxHealth;
    }
    
    void Update() {
        if(Input.GetKeyDown(KeyCode.Alpha4)) {
            HandleTakeDamage();
        }
        if(canMove) {
            HandleMovementInput();
            if(canCrouch) { 
                AttemptToCrouch(); 
            }
            HandleHeadbobEffect();
            HandleStamina();
            //movementSFX.HandleMovementSFX();
            ApplyFinalMovement();

        }
    }

    public void HandleTakeDamage() {
        if(canTakeDamage) {
            canTakeDamage = false;
            currentHealth -= 1;
            damageAudio.Play();

            UpdatePostProcessingEffects();

            if(currentHealth > 0) {
                StartCoroutine(RegenerateHealth());
            } else {
                heartbeatAudio.Stop();
                redBG.SetActive(false);
                bloodUI.enabled = false;
                StopCoroutine(RegenerateHealth());
                gameController.HandlePlayerDeath();
            }
        }
    }

    IEnumerator RegenerateHealth() {
        isRegenerating = true;
        yield return new WaitForSeconds(2f);
        canTakeDamage = true;

        while(currentHealth < maxHealth && currentHealth > 0f) {
            currentHealth += 1 * Time.deltaTime;
            Debug.Log("hp: " + currentHealth);

            if(currentHealth >= maxHealth) {
                currentHealth = maxHealth;
                isRegenerating = false; 
                yield break;
            }

            UpdatePostProcessingEffects();
            HandleLowHealthEffects();
            yield return null;
        }
    }

    void UpdatePostProcessingEffects() {
        float hp = currentHealth / maxHealth;
        float effectStrength = 1f - hp;

        vignette.intensity.value = Mathf.Lerp(0f, maxVignetteIntensity, effectStrength);
        colorAdjustments.saturation.value = Mathf.Lerp(0f, maxSaturation, effectStrength);

        Color newColor = bloodUI.color;
        newColor.a = Mathf.Lerp(0f, 1f, effectStrength);
        bloodUI.color = newColor;
    }

    void HandleLowHealthEffects() {
        if (currentHealth < 1.5f) {
            if (!heartbeatAudio.isPlaying) {
                heartbeatAudio.volume = 0.8f;
                heartbeatAudio.Play();
            }

            if (!bloodUI.enabled) {
                bloodUI.enabled = true;
            }
        }
        else {
            if (heartbeatAudio.isPlaying) {
                StartCoroutine(FadeHeartbeatOut());
            }
        }
    }




    IEnumerator FadeHeartbeatOut() {
        while(heartbeatAudio.volume > 0f) {
            heartbeatAudio.volume -= 0.5f * Time.deltaTime;
            yield return null;
        }
        heartbeatAudio.Stop();
    }

    public IEnumerator HandleDrinkEffect() {
        switch(gameController.chosenDrinkIndex) {
            case 0:
                sprintSpeed += 15;
                yield return new WaitForSeconds(10f);
                sprintSpeed -= 15;
                break;
            case 1:
                maxStamina = 9999f;
                currentStamina = maxStamina;
                yield return new WaitForSeconds(20f);
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
        } else {
            canMove = true;
            fpHighlights.CanInteract = true;
            mouseLook.CanRotateMouse = true;
            flashlight.ToggleFlashlightStatus(true);
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

    public void RotateTowardsSpeaker(GameObject target) {
        Vector3 direction = target.transform.position - tf.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        tf.rotation = Quaternion.Slerp(tf.rotation, rotation, Time.deltaTime * 15f);
        tf.localEulerAngles = new Vector3(0f, tf.localEulerAngles.y, 0);
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
        if(IsSliding) {
            currentMovement += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }
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
    */
}