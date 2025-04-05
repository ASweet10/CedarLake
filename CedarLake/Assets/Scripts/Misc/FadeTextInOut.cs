using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class FadeTextInOut : MonoBehaviour
{
    [SerializeField] TMP_Text textToFade;
    [SerializeField] Color startColor;
    [SerializeField] Color endColor;
    [SerializeField] float lerpTime = 1f;
    void Update() {
        FadeText(textToFade);
    }

    void FadeText(TMP_Text text) {
        Color lerpedColor = Color.Lerp(startColor, endColor, Mathf.PingPong(Time.time, lerpTime));
        text.color = lerpedColor;
        //Debug.Log(lerpedColor);
    }
}
