using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOnTimer : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] float waitTime = 120f;
    bool canPlayAudio = true;

    void Awake() {
        audioSource = gameObject.GetComponent<AudioSource>();
    }
    
    void Update() {
        if(canPlayAudio) {
            StartCoroutine(PlaySoundThenWait());
        }
    }
    
    IEnumerator PlaySoundThenWait() {
        canPlayAudio = false;
        audioSource.Play();
        yield return new WaitForSeconds(waitTime);
        canPlayAudio = true;
    }
}