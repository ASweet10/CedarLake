using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextColorLerper : MonoBehaviour
{
    TextMeshPro textGUI;
    Color startColor = new Color(1f, 1f, 1f, 1f);
    Color endColor = new Color(1f, 1f, 1f, 0f);
    //float duration = 5f;
    //float t = 0f;
    void Start() {
        textGUI = gameObject.GetComponent<TextMeshPro>();
    }
    void Update() {
        LerpColor();
    }

    void LerpColor() {
        Color newColor = Color.Lerp(startColor, endColor, Mathf.PingPong(Time.time, 1));
        textGUI.color = newColor;
    }
}
