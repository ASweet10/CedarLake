using UnityEngine;

public class TwigTrigger : MonoBehaviour
{
    [SerializeField] AudioSource twigAudio;
    [SerializeField] AudioClip[] twigClips;
    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player") {
            if (!twigAudio.isPlaying) {
                twigAudio.clip = twigClips[Random.Range(0, twigClips.Length - 1)];
                twigAudio.Play();
            }
        }
    }
}