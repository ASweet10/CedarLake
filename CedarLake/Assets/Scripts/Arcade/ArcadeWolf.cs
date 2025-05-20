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


    [Header("Obstacle Avoidance")]
    [SerializeField] float obstacleCheckRadius;
    [SerializeField] float obstacleCheckDistance;
    [SerializeField] LayerMask obstacleLayerMask;
    RaycastHit2D[] obstacleCollisions;
    Vector3 targetDirection;
    Vector3 obstacleAvoidanceTargetDirection;
    float obstacleAvoidanceCooldown;

    float moveSpeed = 0.9f;
    float rotationSpeed = 5f;
    float distance;
    public bool CanMove { get; private set; }

    void Start() {
        tf = gameObject.GetComponent<Transform>();
        obstacleCollisions = new RaycastHit2D[10];
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
            distance = Vector2.Distance(tf.position, arcadePlayer.position);
            if(distance > 0.5f) {
                Vector3 playerVector = arcadePlayer.position - tf.position;
                targetDirection = playerVector.normalized; 
                // Vector3 because vector2 didn't account for z-axis in 3d space; Sprite was floating off screen
                //tf.position = Vector3.MoveTowards(tf.position, arcadePlayer.position, moveSpeed * Time.deltaTime);
                tf.position = Vector3.MoveTowards(tf.position, targetDirection, moveSpeed * Time.deltaTime);
                anim.SetBool("isWalking", true);

                HandleObstacles();
            } else {
                arcadeController.HandleLoseArcadeLife();
                anim.SetBool("isWalking", false);
            }
        }
    }

    void HandleObstacles () {

        obstacleAvoidanceCooldown -= Time.deltaTime;

        var contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(obstacleLayerMask);

        int collisions = Physics2D.CircleCast(tf.position, obstacleCheckRadius, transform.up, contactFilter, obstacleCollisions, obstacleCheckDistance);

        for (int i = 0; i < collisions; i++) {
            var obstacleCollision = obstacleCollisions[i];

            if (obstacleCollision.collider.gameObject == gameObject) {
                continue;
            }
            if (obstacleAvoidanceCooldown <= 0) {
                obstacleAvoidanceTargetDirection = obstacleCollision.normal;
                obstacleAvoidanceCooldown = 0.5f;
            }

            var targetRotation = Quaternion.LookRotation(tf.forward, obstacleAvoidanceTargetDirection);
            var rotation = Quaternion.RotateTowards(tf.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            //targetDirection = obstacleCollision.normal;
            targetDirection = rotation * Vector2.up;
            break;
        }
    }

    public void SetCanMove(bool choice) {
        CanMove = choice;
    }
    public void ResetWolfPosition() {
        transform.position = wolfStartPosition.position;
    }
}