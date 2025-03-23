using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAI : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Transform playerTF;
    [SerializeField] Transform killerTF;
    [SerializeField] Transform[] waypoints;
    [SerializeField] float turnSpeed = 60f;
    [SerializeField, Range(1, 5)] float walkSpeed = 2f;

    Transform tf;
    AudioSource footstepAudioSource;
    bool atWaypoint = false;
    bool characterMoving = false;
    int currentWP = 0;
    public enum State{ idle, walkingToWaypoint, talking, hiding, followingPlayer, dead };
    State state = State.idle;
    public State StateRef {
        get { return state; }
        set { state = value; }
    }

    enum CharacterType{ Friend, Hunter, Killer };
    [SerializeField] CharacterType characterType = CharacterType.Hunter;

    void Start () {
        anim = gameObject.GetComponent<Animator>();
        tf = gameObject.GetComponent<Transform>();
        state = State.idle;
    }

    void Update() {
        HandleAIBehavior();
    }
    void FixedUpdate() {
        /*
        if(characterType != CharacterType.Cashier) {
            if(characterMoving) {
                Vector3 direction = waypoints[currentWP].position - tf.position;
                direction.y = 0;
                Quaternion rotation = Quaternion.LookRotation(direction);
                tf.rotation = Quaternion.Slerp(tf.rotation, rotation, Time.deltaTime * turnSpeed);
                tf.position += transform.forward * Time.deltaTime * walkSpeed;

                if(!footstepAudioSource.isPlaying) {
                    footstepAudioSource.Play();
                }
            } else {
                if(footstepAudioSource.isPlaying) {
                    footstepAudioSource.Stop();
                }
            }
        }
        */

    }
    void HandleAIBehavior() {
        switch (state) {
            case State.idle:
                HandleIdle();
                break;
            case State.talking:
                HandleTalking();
                break;
            case State.walkingToWaypoint:
                HandleWaypointNavigation();
                break;
            case State.hiding:
                HandleHideBehavior();
                break;
            case State.followingPlayer:
                HandleFollowPlayer();
                break;
            case State.dead:
                HandleDeath();
                break;
        }
    }

    void HandleWaypointNavigation() {
        if (currentWP != waypoints.Length) {
            if (!atWaypoint) {
                if(!CanRotateTowardsWaypoint(waypoints[currentWP])) {
                    GoToNextWaypoint(waypoints[currentWP]);
                }
            }
        } else {
            state = State.idle;
        }
    }
    
    void GoToNextWaypoint(Transform wp) {
        atWaypoint = false;
        anim.SetBool("isIdle", false);
        anim.SetBool("isWalking", true);

        if(Vector3.Distance(tf.position, wp.position) <= 1f){
            atWaypoint = true;
            currentWP ++;
            if(currentWP >= waypoints.Length) {
                characterMoving = false;
                state = State.idle;
            } else {
                GoToNextWaypoint(waypoints[currentWP]);
            }
        } else {
            characterMoving = true;
        }
    }

    public bool CanRotateTowardsWaypoint(Transform wp) {
        Vector3 nextWPVector = wp.position - tf.position;

        if(nextWPVector != Vector3.zero){
            Quaternion targetRotation = Quaternion.LookRotation(nextWPVector);
            if (targetRotation != transform.rotation) {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed);
                return true; // Rotated, true
            } else {
                return false; // Facing that direction
            }
        } else {
            return false;
        }
    }

    public void RotateAndStartTalking() {
        Vector3 targetPosition = playerTF.position - tf.position;
        Quaternion rotation = Quaternion.LookRotation(targetPosition);
        tf.rotation = Quaternion.Slerp(tf.rotation, rotation, Time.deltaTime * turnSpeed);
        tf.localEulerAngles = new Vector3(0f, tf.localEulerAngles.y, 0);

        state = State.talking;
    }

    void HandleIdle() {
        characterMoving = false;
        anim.SetBool("isWalking", false);
        anim.SetBool("isTalking", false);
        anim.SetBool("isIdle", true);
    }
    void HandleTalking() {
        characterMoving = false;
        anim.SetBool("isWalking", false);
        anim.SetBool("isIdle", false);
        anim.SetBool("isTalking", true);
    }
    void HandleHideBehavior() {
        // find nearest bush you can hide in
        // If killer not within range, hide there
        // If killer within range, run away
    }
    void HandleFollowPlayer() {

    }
    void HandleDeath() {
        anim.SetBool("isWalking", false);
        anim.SetBool("isIdle", false);
        anim.SetBool("isTalking", false);
        anim.SetTrigger("death");
    }
}