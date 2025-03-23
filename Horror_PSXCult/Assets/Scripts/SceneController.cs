using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] GameObject startMenuCamera;
    [SerializeField] Vector3 cameraStartPosition;
    [SerializeField] Vector3 cameraEndPosition;
    [SerializeField] float cameraPanDuration = 5f;


    [SerializeField] float sceneFadeDuration = 2f;
    [SerializeField] Image blackFadeImage;
    public bool isFading;
    float elapsedTime;
    public float elapsedPercentage = 0;


    /*
    IEnumerator Start() {
        if(SceneManager.GetActiveScene().buildIndex == 1) {
            yield return FadeIn();
        }
    }
    */
    void Update() {
        if (Input.GetKeyDown(KeyCode.J)){
            StartCoroutine(FadeOut(2, 1));
        }
        if (Input.GetKeyDown(KeyCode.K)) {
            StartCoroutine(FadeIn(2));
        }
    }

    public IEnumerator FadeIn(float duration) {
        Color imageColor = blackFadeImage.color;
        float fadePercentage;

        blackFadeImage.enabled = true;

        while (blackFadeImage.color.a > 0) {
            fadePercentage = imageColor.a - (duration * Time.deltaTime);

            imageColor = new Color(imageColor.r, imageColor.g, imageColor.b, fadePercentage);
            blackFadeImage.color = imageColor;
            yield return null;  
        }

        blackFadeImage.enabled = false;
    }

    public IEnumerator FadeOut(float duration, int optionalSceneIndex) {
        Color imageColor = blackFadeImage.color;
        float fadePercentage;

        blackFadeImage.enabled = true;

        while (blackFadeImage.color.a < 1) {
            fadePercentage = imageColor.a + (duration * Time.deltaTime);

            imageColor = new Color(imageColor.r, imageColor.g, imageColor.b, fadePercentage);
            blackFadeImage.color = imageColor;
            yield return null;  
        }

        if(optionalSceneIndex != 99) {
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(optionalSceneIndex);
        }
    }



    public IEnumerator PanCameraOnPlay() {
        Vector3 velocity = Vector3.zero;

        while(elapsedTime < cameraPanDuration) {
            startMenuCamera.transform.position = Vector3.SmoothDamp(startMenuCamera.transform.position, cameraEndPosition, ref velocity, 15f, 0.5f);
            elapsedTime += Time.deltaTime;     
        } 

        StartCoroutine(FadeOut(2, 1));
        yield return null;        
    }
}