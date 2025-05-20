using UnityEngine;
using UnityEngine.AI;

public class MovementSFX : MonoBehaviour
{
    Transform tf;
    FirstPersonController fpController;
    CharacterAI characterAI;
    KillerAI killerAI;
    NavMeshAgent agent;

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
    float getCurrentOffset;
    public int terrainDataIndex;
    public enum CharacterType { Player, Character, Killer };
    [SerializeField] CharacterType characterType;

    void Awake() {
        tf = gameObject.GetComponent<Transform>();
        footstepAudioSource = gameObject.GetComponent<AudioSource>();
        terrainTexDetector = gameObject.GetComponent<TerrainTexDetector>();
        if (characterType == CharacterType.Player) {
            fpController = gameObject.GetComponent<FirstPersonController>();
        }
        else if (characterType == CharacterType.Character) {
            agent = gameObject.GetComponent<NavMeshAgent>();
            characterAI = gameObject.GetComponent<CharacterAI>();
        }
        else {
            agent = gameObject.GetComponent<NavMeshAgent>();
            killerAI = gameObject.GetComponent<KillerAI>();
        }
    }
    void Update() {
        DetermineOffset();
        terrainDataIndex = terrainTexDetector.GetActiveTerrainTextureIdx(tf.position);
        HandleMovementSFX();
    }

    public void HandleMovementSFX() {
        footstepTimer -= Time.deltaTime; // Play one footstep per second? what is this

        if(footstepTimer <= 0) {
            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 4)) {
                if(hit.collider.tag == "Tile") {
                    footstepAudioSource.PlayOneShot(concreteClips[Random.Range(0, concreteClips.Length - 1)]);
                } else {
                    switch(terrainDataIndex) {
                        case 0:
                            footstepAudioSource.PlayOneShot(concreteClips[Random.Range(0, concreteClips.Length - 1)]);
                            break;
                        case 3:
                            footstepAudioSource.PlayOneShot(dirtClips[Random.Range(0, dirtClips.Length - 1)]);
                            break;
                        case 4:
                            footstepAudioSource.PlayOneShot(grassClips[Random.Range(0, grassClips.Length - 1)]);
                            break;
                    }
                }
            }
            footstepTimer = getCurrentOffset;
        }
    }

    public void StopSFX() {
        footstepAudioSource.Stop();
    }

    void DetermineOffset() {
        if (characterType == CharacterType.Player) {
            if(!fpController.isMoving) {
                StopSFX();
                return;
            } else {
                getCurrentOffset = fpController.isCrouching ? baseStepSpeed * crouchStepMultiplier : fpController.isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;
            }
        } else if (characterType == CharacterType.Character) {
            if(agent.velocity.magnitude == 0) {
                StopSFX();
            } else {
                getCurrentOffset = characterAI.StateRef == CharacterAI.State.followingPlayer ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;
            }
        } else if (characterType == CharacterType.Killer) {
            if(agent.velocity.magnitude == 0) {
                StopSFX();
            } else {
                getCurrentOffset = killerAI.StateRef == KillerAI.State.chasingPlayer ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;
            }
        }
    }
}