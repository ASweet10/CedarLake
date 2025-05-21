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
    float playerStepSpeed = 0.95f;
    float aiStepSpeed = 0.55f;
    float crouchStepMultiplier = 1.5f;
    float sprintStepMultiplier = 0.7f;
    float footstepTimer = 0;
    float currentOffset;
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
        HandleMovementSFX();
    }


    public void HandleMovementSFX() {
        footstepTimer -= Time.deltaTime; // Play one footstep per second? what is this

        //terrainDataIndex = terrainTexDetector.GetActiveTerrainTextureIdx(tf.position);
        int terrainDataIndex = terrainTexDetector.GetActiveTexture(tf.position);
        
        if(footstepTimer <= 0) {
            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 4)) {
                if(hit.collider.tag == "Tile") {
                    footstepAudioSource.PlayOneShot(concreteClips[Random.Range(0, concreteClips.Length - 1)]);
                    Debug.Log("tile");
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
            footstepTimer = DetermineOffset();
        }
    }

    float DetermineOffset() {
        if (characterType == CharacterType.Player) {
            if(!fpController.isMoving) {
                footstepAudioSource.Stop();
            } else {
                currentOffset = fpController.isCrouching ? playerStepSpeed * crouchStepMultiplier : fpController.isSprinting ? playerStepSpeed * sprintStepMultiplier : playerStepSpeed;
            }
        } else if (characterType == CharacterType.Character) {
            if(agent.velocity.magnitude == 0) {
                footstepAudioSource.Stop();
            } else {
                currentOffset = characterAI.StateRef == CharacterAI.State.followingPlayer ? aiStepSpeed * sprintStepMultiplier : aiStepSpeed;
            }
        } else if (characterType == CharacterType.Killer) {
            if(agent.velocity.magnitude == 0) {
                footstepAudioSource.Stop();
            } else {
                currentOffset = killerAI.StateRef == KillerAI.State.chasingPlayer ? aiStepSpeed * sprintStepMultiplier : aiStepSpeed;
            }
        }

        return currentOffset;
    }
}