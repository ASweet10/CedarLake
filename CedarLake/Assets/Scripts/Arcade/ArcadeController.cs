using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class ArcadeController : MonoBehaviour
{
    [SerializeField] GameObject arcadeWolfObject;
    ArcadeWolf arcadeWolfController;
    [SerializeField] GameObject deathUI;
    [SerializeField] GameObject arcadeLevelOne;
    [SerializeField] GameObject arcadeBloodScreen;
    [SerializeField] GameObject arcadeBackground;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Rigidbody2D playerRB;
    [SerializeField] Animator anim;
    [SerializeField] AudioSource wolfSnarlAudio;
    [SerializeField] Transform arcadePlayerTF;
    [SerializeField] Transform playerStartPosition;
    [SerializeField] Light arcadeLight;
    float moveSpeed = 1.5f;
    int playerLives;
    int maxLives = 3;
    bool canMove;
    public bool CanMove {
        get { return canMove; }
        set { canMove = value; }
    }
    bool wolfSpawned;
    bool facingRight;
    float startTime;

    void Start() {
        wolfSnarlAudio = gameObject.GetComponent<AudioSource>();
        arcadeWolfController = arcadeWolfObject.GetComponent<ArcadeWolf>();

        playerLives = maxLives;
        canMove = true;
        facingRight = true;
        startTime = Time.time;
        wolfSpawned = false;
    }
    void Update() {
        if(Time.time - startTime > 6f) {
            if(!wolfSpawned) {
                arcadeWolfObject.SetActive(true);
                wolfSpawned = true;
            }
        }
        MovePlayerOnInput();
    }
    void MovePlayerOnInput() {
        if(canMove) {
            if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                playerRB.MovePosition(playerRB.position + new Vector2(0, 1) * moveSpeed * Time.deltaTime);
                anim.SetBool("isWalking", true);
            } else if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                playerRB.MovePosition(playerRB.position + new Vector2(-1, 0) * moveSpeed * Time.deltaTime);
                anim.SetBool("isWalking", true);
                if(facingRight) {
                    spriteRenderer.flipX = true;
                    facingRight = false;
                }
            } else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
                playerRB.MovePosition(playerRB.position + new Vector2(0, -1) * moveSpeed * Time.deltaTime);
                anim.SetBool("isWalking", true);
            } else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                playerRB.MovePosition(playerRB.position + new Vector2(1, 0) * moveSpeed * Time.deltaTime);
                anim.SetBool("isWalking", true);
                if(!facingRight) {
                    spriteRenderer.flipX = false;
                    facingRight = true;
                }
            } else { // No movement input
                anim.SetBool("isWalking", false);
            }
        }
    }

    public void HandleLoseArcadeLife() {
        playerLives --;
        if(playerLives <= 0) {
            StartCoroutine(HandleArcadeGameOver());
        } else {
            // screen gets bloodier? some feedback
        }
    }

    public IEnumerator HandleArcadeGameOver() {
        canMove = false;
        anim.SetBool("isWalking", false);
        arcadeLevelOne.SetActive(false);
        wolfSnarlAudio.Play();
        arcadeBackground.SetActive(false);
        arcadeBloodScreen.SetActive(true);
        arcadeLight.enabled = false;
        
        yield return new WaitForSeconds(2f);
        arcadeBloodScreen.SetActive(false);
        arcadeBackground.SetActive(true);
        arcadeLight.enabled = true;
        deathUI.SetActive(true);
    }

    public void ResetArcadePlayerPosition() {
        arcadePlayerTF.position = playerStartPosition.position;
    }
    public void ResetStartTime() {
        startTime = Time.time;
    }

    public IEnumerator DisableMovementAndTransitionScreen() {
        canMove = false;
        arcadeWolfController.CanMove = false;
        yield return new WaitForSeconds(2f);
        canMove = true;
        arcadeWolfController.CanMove = true;
    }
}