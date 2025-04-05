using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadeWolf : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Rigidbody2D wolfRB;
    [SerializeField] ArcadeController arcadeController;
    [SerializeField] Transform wolfStartPosition;
    [SerializeField] Animator anim;
    [SerializeField] Transform arcadePlayer;
    float moveSpeed = 1.1f;
    float distance;
    bool canMove;
    public bool CanMove {
        get { return canMove; }
        set { canMove = value; }
    }


    void Start() {
        ResetWolfPosition();
    }
    void Update() {
        if(arcadePlayer.position.x < transform.position.x) {
            spriteRenderer.flipX = true;
        } else {
            spriteRenderer.flipX = false;
        }
        if(canMove) {
            HandleAIBehavior();
        }

    }
    void HandleAIBehavior() {
        distance = Vector2.Distance(transform.position, arcadePlayer.position);
        if(distance > 0.5f) {
            // Vector3 & not vector2 because vector2 didn't account for z-axis in 3d space
            // Sprite was floating off arcade screen
            transform.position = Vector3.MoveTowards(transform.position, arcadePlayer.position, moveSpeed * Time.deltaTime);
            anim.SetBool("isWalking", true);
        } else {
            arcadeController.HandleLoseArcadeLife();
            anim.SetBool("isWalking", false);
        }
    }

    public void ResetWolfPosition() {
        transform.position = wolfStartPosition.position;
    }
}