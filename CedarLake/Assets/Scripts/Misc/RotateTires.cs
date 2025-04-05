using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTires : MonoBehaviour
{
    Transform wheelTransform;
    public enum RotationDirection {forward, reverse};
    [SerializeField] RotationDirection rotationDirection;
    void Awake() {
        wheelTransform = gameObject.GetComponent<Transform>();
    }
    void Update() {
        RotateTheTire();
    }
    
    void RotateTheTire() {
        if(rotationDirection == RotationDirection.forward){
            wheelTransform.Rotate(new Vector3(360f * Time.deltaTime, 0f, 0f));
        } else {
            wheelTransform.Rotate(new Vector3(-360f * Time.deltaTime, 0f, 0f));
        }
    }
}
