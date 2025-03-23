using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crickets : MonoBehaviour
{
    AudioSource cricketAudio;
    GameObject player;
    Transform tf;
    void Awake() {
        cricketAudio = gameObject.GetComponent<AudioSource>();
        tf = gameObject.GetComponent<Transform>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update() {
        DetectPlayerMovement();
    }

    void DetectPlayerMovement() {
        if(Vector3.Distance(tf.position, player.transform.position) < 7f) {
            cricketAudio.Stop();
        } else {
            if(!cricketAudio.isPlaying) {
                cricketAudio.Play();
            }
        }
    }
}
