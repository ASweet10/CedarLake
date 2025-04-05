using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAI : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Transform playerTF;
    [SerializeField] Transform killerTF;
    [SerializeField] Transform[] waypoints;
    [SerializeField] float turnSpeed = 60f;
    [SerializeField, Range(1, 5)] float walkSpeed = 2f;
    NavMeshAgent agent;

    Transform tf;
    AudioSource footstepAudioSource;
    int currentWP = 0;
    public enum State{ idle, walkingToWaypoint, talking, hiding, followingPlayer, dead };
    State state = State.idle;
    public State StateRef {
        get { return state; }
        set { state = value; }
    }

    void Start () {
        agent = gameObject.GetComponent<NavMeshAgent>();
        anim = gameObject.GetComponent<Animator>();
        tf = gameObject.GetComponent<Transform>();
        state = State.followingPlayer;
    }

    void Update() {
        HandleAIBehavior();
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
        if (currentWP != waypoints.Length - 1) {
            if (Vector3.Distance(transform.position, waypoints[currentWP].position) > 1f) {
                anim.SetBool("walking", true);
                agent.SetDestination(waypoints[currentWP].position);
                
            } else {
                currentWP++;
            }
        } else {
            anim.SetBool("walking", false);
            state = State.idle;
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
        anim.SetBool("idle", true);
        agent.isStopped = true;

        //if character injured check...
        if (Vector3.Distance(transform.position, playerTF.position) > 3f) {
            anim.SetBool("idle", false);
            agent.isStopped = false;
            state = State.followingPlayer;
        }
    }
    void HandleTalking() {
        anim.SetBool("talking", true);
    }
    void HandleHideBehavior() {
        // find nearest bush you can hide in
        // If killer not within range, hide there
        // If killer within range, run away
    }
    void HandleFollowPlayer() {
        if (Vector3.Distance(transform.position, playerTF.position) > 5f) {
            anim.SetBool("injured", true);
            agent.SetDestination(playerTF.position);
        } else {
            anim.SetBool("injured", false);
            state = State.idle;
        }
    }
    void HandleDeath() {
        anim.SetTrigger("death");
    }



    /*
    void GoToNextWaypoint(Transform wp) {
        atWaypoint = false;
        anim.SetBool("isIdle", false);
        anim.SetBool("isWalking", true);

        if(Vector3.Distance(tf.position, wp.position) <= 1f){
            atWaypoint = true;
            currentWP ++;
            if(currentWP >= waypoints.Length) {
                state = State.idle;
            } else {
                GoToNextWaypoint(waypoints[currentWP]);
            }
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
    */
}