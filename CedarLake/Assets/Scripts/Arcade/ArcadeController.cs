using System.Collections;
using TMPro;
using UnityEngine;

public class ArcadeController : MonoBehaviour {
    [SerializeField] GameObject arcadeWolfObject;
    [SerializeField] ArcadeCamera arcadeCamera;
    [SerializeField] GameObject startScreen;
    [SerializeField] GameObject deathScreen;
    [SerializeField] GameObject winGameScreen;
    [SerializeField] GameObject arcadeLevelOne;
    [SerializeField] GameObject arcadeLevelTwo;
    [SerializeField] GameObject arcadeLevelScreen;
    [SerializeField] GameObject arcadeBloodScreen;
    [SerializeField] GameObject arcadeBackground;
    [SerializeField] GameObject arcadePlayerObject;
    [SerializeField] SpriteRenderer arcadeSpriteRenderer;
    [SerializeField] Rigidbody2D arcadeRB;
    [SerializeField] Animator arcadeAnim;
    [SerializeField] AudioSource wolfSnarlAudio;
    [SerializeField] Transform arcadePlayerTF;
    [SerializeField] Transform wolfTF;
    [SerializeField] Transform playerStartPosition;
    [SerializeField] Transform wolfStartPosition;
    [SerializeField] Light arcadeLight;
    [SerializeField] Camera arcadeStartCam;
    [SerializeField] Camera arcadePlayerCam;

    [SerializeField] FirstPersonController fpController;
    [SerializeField] Camera mainGameCamera;
    [SerializeField] AudioSource arcadeCoinSound;
    [SerializeField] AudioClip arcadeCoinSFX;

    float moveSpeed = 1.6f;
    int playerLives;
    int maxLives = 3;
    bool canMove;
    public bool CanMove {
        get { return canMove; }
        set { canMove = value; }
    }
    bool facingRight;
    public bool playingArcadeGame;


    void Start() {
        playingArcadeGame = false;
        wolfSnarlAudio = gameObject.GetComponent<AudioSource>();

        playerLives = maxLives;
        canMove = false;
        facingRight = true;
    }
    void Update() {
        HandleArcadeMovement();
    }
    void HandleArcadeMovement() {
        if (canMove) {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                arcadeRB.MovePosition(arcadeRB.position + new Vector2(0, 1) * moveSpeed * Time.deltaTime);
                arcadeAnim.SetBool("isWalking", true);
            } else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                arcadeRB.MovePosition(arcadeRB.position + new Vector2(-1, 0) * moveSpeed * Time.deltaTime);
                arcadeAnim.SetBool("isWalking", true);
                if (facingRight) {
                    arcadeSpriteRenderer.flipX = true;
                    facingRight = false;
                }
            } else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
                arcadeRB.MovePosition(arcadeRB.position + new Vector2(0, -1) * moveSpeed * Time.deltaTime);
                arcadeAnim.SetBool("isWalking", true);
            } else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                arcadeRB.MovePosition(arcadeRB.position + new Vector2(1, 0) * moveSpeed * Time.deltaTime);
                arcadeAnim.SetBool("isWalking", true);
                if (!facingRight) {
                    arcadeSpriteRenderer.flipX = false;
                    facingRight = true;
                }
            } else { // No movement input
                arcadeAnim.SetBool("isWalking", false);
            }
        }
    }

    public void HandleArcadeGameOver() {
        StartCoroutine(EnableGameOver());
    }

    public IEnumerator EnableGameOver() {
        canMove = false;
        wolfSnarlAudio.Play();

        arcadeLevelOne.SetActive(false);
        arcadePlayerObject.SetActive(false);
        arcadeWolfObject.SetActive(false);
        arcadePlayerCam.enabled = false;
        arcadeStartCam.enabled = true;
        arcadeBackground.SetActive(false);
        arcadeBloodScreen.SetActive(true);
        arcadeLight.enabled = false;

        yield return new WaitForSeconds(1f);
        arcadeBloodScreen.SetActive(false);
        arcadeBackground.SetActive(true);
        arcadeLight.enabled = true;
        deathScreen.SetActive(true);

        yield return new WaitForSeconds(3f);
        deathScreen.SetActive(false);
        startScreen.SetActive(true);

        StartCoroutine(HandleEnableArcade(false));
    }

    public void HandleLevelTransition(int level) {
        if (level == 1) {
            StartCoroutine(EnableSceneTwo());
        } else {
            StartCoroutine(HandleWinGame());
        }
    }

    public IEnumerator EnableSceneTwo() {
        canMove = false;
        arcadeLevelOne.SetActive(false);
        arcadePlayerCam.enabled = false;
        arcadeStartCam.enabled = true;
        arcadeLevelScreen.SetActive(true);
        arcadeLevelScreen.GetComponentInChildren<TextMeshPro>().text = "II";

        yield return new WaitForSeconds(2f);
        arcadeLevelScreen.SetActive(false);
        arcadeLevelTwo.SetActive(true);
        arcadeStartCam.enabled = false;
        arcadePlayerCam.enabled = true;

        ResetCharacterPositions();
        canMove = true;
    }

    public IEnumerator HandleWinGame() {
        canMove = false;

        arcadeLevelTwo.SetActive(false);
        arcadePlayerObject.SetActive(false);
        arcadeWolfObject.SetActive(false);
        arcadePlayerCam.enabled = false;
        arcadeStartCam.enabled = true;
        winGameScreen.SetActive(true);

        yield return new WaitForSeconds(2.5f);
        winGameScreen.SetActive(false);
        startScreen.SetActive(true);

        StartCoroutine(HandleEnableArcade(false));
    }

    public void ResetCharacterPositions() {
        arcadePlayerObject.SetActive(true);
        arcadeWolfObject.SetActive(true);
        facingRight = true;
        arcadeSpriteRenderer.flipX = false;
        arcadePlayerTF.position = playerStartPosition.position;
        wolfTF.position = wolfStartPosition.position;
        CanMove = true;
    }

    public IEnumerator HandleEnableArcade(bool enabled) {
        if (enabled) {
            arcadeStartCam.enabled = true;
            mainGameCamera.enabled = false;
            fpController.DisablePlayerMovement(true, false);

            arcadeCoinSound.Play();

            yield return new WaitForSeconds(1f);
            startScreen.SetActive(false);
            arcadeLevelScreen.SetActive(true);
            arcadeLevelScreen.GetComponentInChildren<TextMeshPro>().text = "I";

            yield return new WaitForSeconds(2f);
            arcadeLevelScreen.SetActive(false);
            arcadeLevelOne.SetActive(true);
            arcadePlayerCam.enabled = true;
            arcadeStartCam.enabled = false;

            ResetCharacterPositions();

            canMove = true;
        } else {
            canMove = false;
            mainGameCamera.enabled = true;
            arcadeStartCam.enabled = false;
            arcadePlayerCam.enabled = false;

            fpController.DisablePlayerMovement(false, false);
            arcadeCoinSound.Stop();
            arcadeCoinSound.clip = arcadeCoinSFX; // reset for next game

            arcadeLevelOne.SetActive(false);
            arcadeLevelTwo.SetActive(false);

            if (deathScreen.activeInHierarchy) {
                deathScreen.SetActive(false);
            }
            startScreen.SetActive(true);
        }
    }
}