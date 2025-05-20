using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class KillerAI : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Transform playerTF;
    Transform tf;
    FieldOfView fovScript;
    CharacterController controller;
    NavMeshAgent agent;
    [SerializeField] Transform[] waypoints;
    int currentWaypoint = 0;


    [Header("Parameters")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float sprintSpeed = 12f;
    [SerializeField] float attackRange = 3f;
    [SerializeField] float chaseRange = 15f;
    //[SerializeField] float hearingRange = 15f;
    public Vector3 playerLastSeenPosition;

    public enum State{ idle, patrolling, lookingAtWaypoint, chasingPlayer, attacking, searchingLastPosition };
    public State state = State.patrolling;
    public State StateRef {
        get { return state; }
        set { state = value; }
    }

    void Start () {
        agent = gameObject.GetComponent<NavMeshAgent>();
        playerTF = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        fovScript = gameObject.GetComponent<FieldOfView>();
        anim = gameObject.GetComponent<Animator>();
        tf = gameObject.GetComponent<Transform>();
        controller = gameObject.GetComponent<CharacterController>();
        currentWaypoint = 0;
    }

    void Update() {
        Debug.DrawLine(Vector3.zero, new Vector3(0, 5, 0), Color.cyan, 30f, false);
        HandleAIBehavior();
    }

    void HandleAIBehavior() {
        switch (state) {
            case State.idle:
                HandleIdle();
                break;
            case State.patrolling:
                HandlePatrol();
                break;
            case State.lookingAtWaypoint:
                HandleLookAtWaypoint();
                break;
            case State.chasingPlayer:
                HandleChaseTarget();
                break;
            case State.attacking:
                HandleAttack();
                break;
            case State.searchingLastPosition:
                HandleSearchLastPosition();
                break;
            default:
                break;
        }
    }

    void HandleIdle() {
        anim.SetBool("chasing", false);
        anim.SetBool("attacking", false);
        anim.SetBool("walking", false);
        anim.SetBool("idle", true);

        if(Vector3.Distance(tf.position, playerTF.position) <= 20f) {
            if(Vector3.Distance(tf.position, playerTF.position) <= attackRange) {
                state = State.attacking;
            } else {
                if(fovScript.canSeePlayer) {
                    state = State.chasingPlayer;
                }
            }
        }
    }

    void HandlePatrol() {
        if(fovScript.canSeePlayer) {
            anim.SetBool("walking", false);
            state = State.chasingPlayer;
        }

        if (Vector3.Distance(tf.position, waypoints[currentWaypoint].position) > 1f) {
            anim.SetBool("walking", true);

            Vector3 waypointPos = waypoints[currentWaypoint].position - tf.position;
            waypointPos = waypointPos.normalized * walkSpeed;
            
            /*
            if(!controller.isGrounded){
                waypointPos.y -= 9.8f * Time.deltaTime; // Apply gravity
                if(controller.velocity.y < -1 && controller.isGrounded){  //Landing frame; reset y value to 0
                    waypointPos.y = 0;
                }
            }
            controller.Move(waypointPos * Time.deltaTime);
            */
            agent.speed = walkSpeed;
            agent.SetDestination(waypoints[currentWaypoint].position);
        } else {
            if(currentWaypoint == waypoints.Length - 1) {
                currentWaypoint = 0;
            } else {
                currentWaypoint ++;
            }
            anim.SetBool("walking", false);
            state = State.lookingAtWaypoint;
        }
    }
    
    void HandleChaseTarget() {
        playerLastSeenPosition = playerTF.position;

        if(Vector3.Distance(tf.position, playerTF.position) < chaseRange) {
            //tf.LookAt(playerTF);
            //tf.localEulerAngles = new Vector3(0f, tf.localEulerAngles.y, tf.localEulerAngles.z);
            anim.SetBool("walking", false);
            anim.SetBool("chasing", true);

            /*
            Vector3 currentMovement = playerTF.position - tf.position;
            currentMovement = currentMovement.normalized * sprintSpeed;
            if(!controller.isGrounded) {
                currentMovement.y -= 9.8f; // Apply gravity
                if(controller.velocity.y < -1 && controller.isGrounded){  //Landing frame; reset y value to 0
                    currentMovement.y = 0;
                }
            }
            */
            //controller.Move(currentMovement * Time.deltaTime);
            agent.speed = sprintSpeed;
            agent.SetDestination(playerTF.position);

            if(Vector3.Distance(tf.position, playerTF.position) <= attackRange) {
                anim.SetBool("chasing", false);
                state = State.attacking;
            }

        } else {
            anim.SetBool("chasing", false);
            state = State.searchingLastPosition;
        }
    }

    void HandleAttack() {
        if(Vector3.Distance(tf.position, playerTF.position) <= attackRange) {
            tf.LookAt(playerTF);
            tf.localEulerAngles = new Vector3(0f, tf.localEulerAngles.y, tf.localEulerAngles.z);
            anim.SetBool("attacking", true);
        } else {
            anim.SetBool("attacking", false);
            state = State.chasingPlayer;
        }
    }
    void HandleSearchLastPosition() {  // Where did I last see player?; Investigate area
        if(fovScript.canSeePlayer) {
            anim.SetBool("walking", false);
            state = State.chasingPlayer;
        }
        
        agent.speed = walkSpeed;
        anim.SetBool("walking", true);

        if(Vector3.Distance(tf.position, playerLastSeenPosition) > 2) {
            agent.SetDestination(playerLastSeenPosition);
            Debug.Log("moving to lastpos");
        } else {
            Vector3 searchPosition = new Vector3(playerLastSeenPosition.x + 3, playerLastSeenPosition.y - 3, playerLastSeenPosition.z + 3);
            StartCoroutine(InvestigateLastPosition(searchPosition));
        }
    }
    
    public void DisableFOV(bool choice) {
        if (choice) {
            fovScript.enabled = false;
        } else {
            fovScript.enabled = true;
        }
    }

    IEnumerator InvestigateLastPosition(Vector3 searchPos) {
        if(Vector3.Distance(tf.position, searchPos) > 1) {
            agent.SetDestination(searchPos);
            Debug.Log("moving to searchpos");
        } else {
            transform.rotation = Quaternion.Slerp(tf.rotation, Quaternion.Euler(0, 90, 0), 5f * Time.deltaTime);
        }
        yield return new WaitForSeconds(1f);
        anim.SetBool("walking", false);
        state = State.patrolling;

        yield break;
    }

    void HandleLookAtWaypoint() {
        if(fovScript.canSeePlayer) {
            anim.SetBool("idle", false);
            state = State.chasingPlayer;
        }
        anim.SetBool("idle", true);

        StartCoroutine(ResumePatrollingAfterDelay());
    }
    IEnumerator ResumePatrollingAfterDelay() {
        yield return new WaitForSeconds(4f);
        anim.SetBool("idle", false);
        state = State.patrolling;
    }
}