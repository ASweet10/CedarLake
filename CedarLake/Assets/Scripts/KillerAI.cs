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
    float lastUpdateTime = 0f;

    public enum State { idle, patrolling, lookingAtWaypoint, chasingPlayer, attacking, searchingLastPosition, pauseIdle };
    public State state = State.patrolling;
    public State StateRef
    {
        get { return state; }
        set { state = value; }
    }
    bool isAttacking;
    float attackTimer = 0f;

    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        playerTF = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        fovScript = gameObject.GetComponent<FieldOfView>();
        anim = gameObject.GetComponent<Animator>();
        tf = gameObject.GetComponent<Transform>();
        controller = gameObject.GetComponent<CharacterController>();
        currentWaypoint = 0;
        isAttacking = true;
        agent.updateRotation = false;
    }

    void Update() {
        Debug.DrawLine(Vector3.zero, new Vector3(0, 5, 0), Color.cyan, 30f, false);
        
        if (fovScript.canSeePlayer) {
            if (Time.time - lastUpdateTime >= 1.5f) {
                playerLastSeenPosition = playerTF.position;
                lastUpdateTime = Time.time;
            }
        }
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
            case State.chasingPlayer:
                HandleChaseTarget();
                break;
            case State.attacking:
                HandleAttack();
                break;
            case State.searchingLastPosition:
                HandleSearchLastPosition();
                break;
            case State.lookingAtWaypoint:
                StartCoroutine(HandleLookAtWaypoint());
                break;
            case State.pauseIdle:
                HandlePauseIdle();
                break;
            default:
                break;
        }
        Debug.Log("killer state: " + state);
    }

    void HandleIdle() {
        anim.SetBool("chasing", false);
        anim.SetBool("attacking", false);
        anim.SetBool("walking", false);
        anim.SetBool("idle", true);

        if (Vector3.Distance(tf.position, playerTF.position) <= 20f)
        {
            if (Vector3.Distance(tf.position, playerTF.position) <= attackRange)
            {
                state = State.attacking;
            }
            else
            {
                if (fovScript.canSeePlayer)
                {
                    state = State.chasingPlayer;
                }
            }
        }
    }

    void HandlePatrol() {
        if (fovScript.canSeePlayer) {
            anim.SetBool("walking", false);
            state = State.chasingPlayer;
        }

        if (Vector3.Distance(tf.position, waypoints[currentWaypoint].position) > 2f)
        {
            Debug.Log(Vector3.Distance(tf.position, waypoints[currentWaypoint].position));
            anim.SetBool("walking", true);

            agent.speed = walkSpeed;
            agent.SetDestination(waypoints[currentWaypoint].position);

            Vector3 direction = (waypoints[currentWaypoint].position - tf.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                Vector3 euler = lookRotation.eulerAngles;
                euler.y += 50f;

                tf.rotation = Quaternion.Slerp(tf.rotation, Quaternion.Euler(euler), Time.deltaTime * 10f);
            }
        }
        else
        {
            if (currentWaypoint == waypoints.Length - 1)
            {
                currentWaypoint = 0;
            }
            else
            {
                currentWaypoint++;
            }
            
            anim.SetBool("walking", false);
            state = State.lookingAtWaypoint;
        }
    }

    void HandleChaseTarget() {
        if (Vector3.Distance(tf.position, playerTF.position) < chaseRange) {
            anim.SetBool("walking", false);
            anim.SetBool("chasing", true);

            if (Time.time - lastUpdateTime >= 1.5f) {
                playerLastSeenPosition = playerTF.position;
                lastUpdateTime = Time.time;
                Debug.Log(playerLastSeenPosition);
            }
            agent.speed = sprintSpeed;
            agent.SetDestination(playerTF.position);

            Vector3 direction = (playerTF.position - tf.position).normalized;

            if (direction != Vector3.zero) {
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

                Vector3 euler = lookRotation.eulerAngles;
                euler.y += 50f;

                //tf.rotation = Quaternion.Slerp(tf.rotation, lookRotation, Time.deltaTime * 10f);
                tf.rotation = Quaternion.Slerp(tf.rotation, Quaternion.Euler(euler), Time.deltaTime * 10f);
            }

            if (Vector3.Distance(tf.position, playerTF.position) <= attackRange)
            {
                anim.SetBool("chasing", false);
                state = State.attacking;
            }

        }
        else {
            anim.SetBool("chasing", false);
            state = State.searchingLastPosition;
        }
    }

    void HandleAttack()
    {
        float distance = Vector3.Distance(tf.position, playerTF.position);

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                isAttacking = false;
                anim.SetBool("attacking", false);
                state = State.chasingPlayer;
            }
        }
        else
        {
            if (distance <= attackRange)
            {
                isAttacking = true;
                attackTimer = 2f;
                tf.LookAt(playerTF);
                tf.localEulerAngles = new Vector3(0f, tf.localEulerAngles.y, tf.localEulerAngles.z);
                anim.SetBool("attacking", true);
            }
            else
            {
                anim.SetBool("attacking", false);
                state = State.chasingPlayer;
            }
        }
    }

    void HandleSearchLastPosition() {  // Where did I last see player?; Investigate area
        if (fovScript.canSeePlayer) {
            anim.SetBool("walking", false);
            state = State.chasingPlayer;
        }

        agent.speed = walkSpeed;
        anim.SetBool("walking", true);

        Vector3 direction = (playerLastSeenPosition - tf.position).normalized;

        if (direction != Vector3.zero) {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            Vector3 euler = lookRotation.eulerAngles;
            euler.y += 50f;

            tf.rotation = Quaternion.Slerp(tf.rotation, Quaternion.Euler(euler), Time.deltaTime * 10f);
        }
        
        if (Vector3.Distance(tf.position, playerLastSeenPosition) > 3f) {
            Debug.Log("distance: " + Vector3.Distance(tf.position, playerLastSeenPosition));
            agent.SetDestination(playerLastSeenPosition);
        }
        else {
            anim.SetBool("walking", false);
            StartCoroutine(InvestigateLastPosition());
        }
    }

    IEnumerator InvestigateLastPosition() {
        anim.SetBool("looking", true);

        float timer = 0f;
        while (timer < 3f) {
            tf.Rotate(0f, 60f * Time.deltaTime, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        anim.SetBool("looking", false);
        state = State.patrolling;
        yield break;
    }

    IEnumerator HandleLookAtWaypoint()
    {
        anim.SetBool("looking", true);

        float timer = 0f;
        while (timer < 2.5f)
        {
            tf.Rotate(0f, 60f * Time.deltaTime, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        if (fovScript.canSeePlayer)
        {
            anim.SetBool("walking", false);
            state = State.chasingPlayer;
        }

        anim.SetBool("looking", false);
        state = State.patrolling;
        yield break;
    }

    void HandlePauseIdle() {
        anim.SetBool("idle", true);
    }
    
    public void DisableKillerMovement(bool disableMovement) {
        if (disableMovement) {
            anim.SetBool("attacking", false);
            anim.SetBool("chasing", false);
            state = State.pauseIdle;
        }
        else {
            state = State.idle;
        }
    }
}