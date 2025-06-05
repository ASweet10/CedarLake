using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering; // post-processing

public class FirstPersonHealth : MonoBehaviour
{
    GameController gameController;
    [SerializeField] AudioSource damageAudio;
    [SerializeField] AudioSource heartbeatAudio;
    [SerializeField] RawImage bloodUI;
    [SerializeField] GameObject redBG;
    [SerializeField] Volume volume;
    Vignette vignette;
    ColorAdjustments colorAdjustments;
    float maxHealth = 3;
    float currentHealth;
    [SerializeField] float maxVignetteIntensity = 0.7f;
    [SerializeField] float maxSaturation = 75f;
    bool canTakeDamage = true;

    void Start() {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        currentHealth = maxHealth;
    }
    void Awake() {
        volume = GameObject.FindGameObjectWithTag("GameController").GetComponent<Volume>();
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out colorAdjustments);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            HandleTakeDamage();
        }
        Debug.Log(currentHealth);
    }
    
        public void HandleTakeDamage() {
        if(canTakeDamage) {
            canTakeDamage = false;
            currentHealth -= 1.3f;
            damageAudio.Play();

            UpdatePostProcessingEffects();

            if(currentHealth > 0) {
                StartCoroutine(RegenerateHealth());
            } else {
                heartbeatAudio.Stop();
                redBG.SetActive(false);
                bloodUI.enabled = false;
                StopCoroutine(RegenerateHealth());
                gameController.HandlePlayerDeath();
            }
        }
    }

    IEnumerator RegenerateHealth() {
        yield return new WaitForSeconds(2f);
        canTakeDamage = true;

        while(currentHealth < maxHealth && currentHealth > 0f) {
            currentHealth += 0.1f * Time.deltaTime;

            if(currentHealth >= maxHealth) {
                currentHealth = maxHealth;
                yield break;
            }

            UpdatePostProcessingEffects();
            HandleLowHealthEffects();
            yield return null;
        }
    }

    void UpdatePostProcessingEffects() {
        float hp = currentHealth / maxHealth;
        float effectStrength = 1f - hp;

        vignette.intensity.value = Mathf.Lerp(0f, maxVignetteIntensity, effectStrength);
        colorAdjustments.saturation.value = Mathf.Lerp(0f, maxSaturation, effectStrength);

        Color newColorOne = bloodUI.color;
        newColorOne.a = Mathf.Lerp(0f, 1f, effectStrength);
        bloodUI.color = newColorOne;
    }

    void HandleLowHealthEffects() {
        if (currentHealth < 1.5f) {
            if (!heartbeatAudio.isPlaying) {
                heartbeatAudio.volume = 0.8f;
                heartbeatAudio.Play();
            }

            if (!bloodUI.enabled) {
                bloodUI.enabled = true;
            }
        } else {
            if (heartbeatAudio.isPlaying) {
                StartCoroutine(FadeHeartbeatOut());
            }
        }
    }


    IEnumerator FadeHeartbeatOut() {
        while(heartbeatAudio.volume > 0f) {
            heartbeatAudio.volume -= 0.5f * Time.deltaTime;
            yield return null;
        }
        heartbeatAudio.Stop();
    }
}
