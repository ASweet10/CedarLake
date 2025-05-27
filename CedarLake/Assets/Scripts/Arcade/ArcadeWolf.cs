using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ArcadeWolf : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] ArcadeController arcadeController;
    [SerializeField] Transform wolfStartPosition;
    [SerializeField] Animator anim;
    [SerializeField] Transform arcadePlayer;
    Transform tf;

    [SerializeField] LayerMask obstacleLayerMask;
    float avoidDistance = 1.5f;
    float moveSpeed = 0.9f;
    public bool CanMove { get; private set; }

    void Start() {
        tf = gameObject.GetComponent<Transform>();
        ResetWolfPosition();
        CanMove = true;
    }
    void Update() {
        HandleWolfAIBehavior();
    }

    void HandleWolfAIBehavior() {
        if(arcadePlayer.position.x < tf.position.x) {
            spriteRenderer.flipX = true;
        } else {
            spriteRenderer.flipX = false;
        }

        if(CanMove) {
            float distance = Vector2.Distance(tf.position, arcadePlayer.position);
            if(distance > 0.5f) {
                Vector3 direction = (arcadePlayer.position - tf.position).normalized;

                bool obstacleAhead = Physics.Raycast(tf.position, direction, avoidDistance, obstacleLayerMask);
                Vector3 rightDirection = Quaternion.Euler(0, 45, 0) * direction;
                Vector3 leftDirection = Quaternion.Euler(0, -45, 0) * direction;

                bool obstacleRight = Physics.Raycast(tf.position, rightDirection, avoidDistance, obstacleLayerMask);
                bool obstacleLeft = Physics.Raycast(tf.position, leftDirection, avoidDistance, obstacleLayerMask);

                Vector3 moveDir = direction;

                if (obstacleAhead) {
                    if (!obstacleLeft)
                        moveDir = leftDirection;
                    else if (!obstacleRight)
                        moveDir = rightDirection;
                    else
                        moveDir = -direction;
                }

                // Vector3 because vector2 didn't account for z-axis in 3d space; Sprite was floating off screen
                tf.position = Vector3.MoveTowards(tf.position, tf.position + moveDir, moveSpeed * Time.deltaTime);
                anim.SetBool("isWalking", true);
            } else {
                arcadeController.HandleLoseArcadeLife();
                anim.SetBool("isWalking", false);
            }
        }
    }

    public void SetCanMove(bool choice)
    {
        CanMove = choice;
    }
    public void ResetWolfPosition() {
        transform.position = wolfStartPosition.position;
    }
}