using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMenuItem : MonoBehaviour
{
    Transform tf;
    public enum RotateMode {x, y, z};
    public RotateMode rotateMode = RotateMode.y;
    void Awake() {
        tf = gameObject.GetComponent<Transform>();
    }

    void Update() {
        RotateItem();
    }
    void RotateItem() {
        if(rotateMode == RotateMode.y) {
            tf.Rotate(new Vector3(0f, 90f * Time.deltaTime * 0.5f, 0f));
        } else if (rotateMode == RotateMode.z) {
            tf.Rotate(new Vector3(0f, 0f, 90f * Time.deltaTime * 0.5f));
        }
    }
}