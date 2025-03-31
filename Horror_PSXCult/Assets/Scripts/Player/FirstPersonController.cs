using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class FirstPersonController : MonoBehaviour
{
    CharacterController controller;
    Transform tf;
    [SerializeField] Camera mainCamera;
    [SerializeField] MouseLook mouseLook;

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
    bool isSprinting => canSprint && Input.GetKey(sprintKey);
    bool isMoving => Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;



    [Header("Footsteps")]
    TerrainTexDetector terrainTexDetector;
    AudioSource footstepAudioSource;
    [SerializeField] AudioClip[] grassClips;
    [SerializeField] AudioClip[] concreteClips;
    [SerializeField] AudioClip[] dirtClips;
    float baseStepSpeed = 0.5f;
    float crouchStepMultiplier = 1.5f;
    float sprintStepMultiplier = 0.6f;
    float footstepTimer = 0;
    float GetCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;
    public int terrainDataIndex;



    [Header("Health")]
    [SerializeField] AudioSource damageAudio;
    [SerializeField] GameObject bleedingUI;
    [SerializeField] RawImage bloodParticles;
    Volume volume;
    Vignette vignette = null;
    float lastDamageTime;
    float maxHealth = 10;
    float currentHealth;
    bool canTakeDamage = true;



    [Header("Stamina")]
    [SerializeField, Range(1, 20)] float maxStamina = 15f;
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
    bool isCrouching;
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
        footstepAudioSource = gameObject.GetComponent<AudioSource>();
        controller = gameObject.GetComponent<CharacterController>();
        terrainTexDetector = gameObject.GetComponent<TerrainTexDetector>();
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        defaultYPosition = mainCamera.transform.localPosition.y; // Return camera to default position when not moving
        tf = gameObject.GetComponent<Transform>();

        volume = GameObject.FindGameObjectWithTag("GameController").GetComponent<Volume>();
        //volume.profile.TryGetSettings(out vignette);
    }

    void Start() {
        DisablePlayerMovement(false, false);
        currentStamina = maxStamina;
        currentHealth = maxHealth;
    }
    
    void Update() {
        if(Input.GetKeyDown(KeyCode.L)) {
            StartCoroutine(TakeDamageAndWait(2));
        }
        if(canMove) {
            //terrainDataIndex = terrainTexDetector.GetActiveTerrainTextureIdx(tf.position);
            HandleMovementInput();
            if(canCrouch) { 
                AttemptToCrouch(); 
            }
            HandleHeadbobEffect();
            HandleStamina();
            HandleMovementSFX();
            ApplyFinalMovement();
        }

        //Debug.Log("current hp: " + currentHealth);
    }
   

    public void TakeAxeDamage() {
        if(canTakeDamage) {
            StartCoroutine(TakeDamageAndWait(2));
        }
    }
    IEnumerator TakeDamageAndWait(int damageValue) {
        canTakeDamage = false;
        lastDamageTime = Time.time;
        currentHealth --;
        damageAudio.Play();

        switch(currentHealth) {
            case float currentHealth when currentHealth > 6 && currentHealth < 10:
                //vignette.enabled.Override(true);
                //vignette.intensity.value = 0.4f;
                vignette.intensity.Override(0.6f);
                /*
                float alpha2 = 0.2f;
                Color particleColor = bloodParticles.color;
                particleColor.a = alpha2;
                bloodParticles.color = particleColor;
                */
                break;
            case float currentHealth when currentHealth > 3 && currentHealth < 6:
                //vignette.enabled.Override(true);
                vignette.intensity.Override(0.6f);
                /*
                float alpha1 = 1f;
                particleColor = bloodParticles.color;
                particleColor.a = alpha1;
                bloodParticles.color = particleColor;
                */
                // heavy breathing audio? 
                break;
            case float currentHealth when currentHealth <= 0:
                //vignette.enabled.Override(false);
                StopCoroutine(HandleRegenerateHealth());
                StartCoroutine(gameController.HandlePlayerDeath());
                break;
        }
        yield return new WaitForSeconds(2);
        StartCoroutine(HandleRegenerateHealth());
        canTakeDamage = true;
    }

    void HandleBleedEffect() {
        if(bleedingUI.activeInHierarchy && Time.time - lastDamageTime > 3f) {
            while (bloodParticles.color.a > 0) {
                bloodParticles.color = new Color(bloodParticles.color.r, bloodParticles.color.g, bloodParticles.color.b, bloodParticles.color.a - (Time.deltaTime / 6));
                if(!canTakeDamage) {
                    break;
                }
            }
            if(canTakeDamage) {
                bleedingUI.SetActive(false);
            }
        }
    }
    IEnumerator HandleRegenerateHealth() {
        while(currentHealth < maxHealth) {
            if(!canTakeDamage) {
                break;
            }
            currentHealth += 1 * Time.deltaTime;
            if(currentHealth >= maxHealth) {
                currentHealth = maxHealth;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void HandleMovementSFX() {
        if(!controller.isGrounded || !isMoving) {
            footstepAudioSource.Stop();
            return;
        }

        footstepTimer -= Time.deltaTime; // Play one footstep per second? what is this

        if(footstepTimer <= 0) {
            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 4)) {
                if(hit.collider.tag == "Tile") {
                    //Debug.Log("tile");
                    footstepAudioSource.PlayOneShot(concreteClips[Random.Range(0, concreteClips.Length - 1)]);
                } else {
                    /*
                    switch(terrainDataIndex) {
                        case 0:
                            footstepAudioSource.PlayOneShot(concreteClips[Random.Range(0, concreteClips.Length - 1)]);
                            break;
                        case 1:
                            footstepAudioSource.PlayOneShot(dirtClips[Random.Range(0, dirtClips.Length - 1)]);
                            break;
                        case 5:
                            footstepAudioSource.PlayOneShot(grassClips[Random.Range(0, grassClips.Length - 1)]);
                            break;
                    }
                    */
                }
            }

            footstepTimer = GetCurrentOffset;
        }

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

    public void RotateTowardsSpeaker(GameObject target) {
        Debug.Log("test");
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