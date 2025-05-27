using UnityEngine;

public class WheelManager : MonoBehaviour
{
    [SerializeField] Transform[] wheels;
    public enum RotationDirection { forward, reverse };
    [SerializeField] RotationDirection rotationDirection = RotationDirection.forward;
    bool isDecelerating = false;
    bool isSpinning = false;
    float spinSpeed = 360f; // degrees/sec
    float decelerationRate = 500f;

    void Update() {
        if (isSpinning) {
            RotateWheels();
        }

        if (isDecelerating) {
            spinSpeed -= decelerationRate * Time.deltaTime;
            if (spinSpeed <= 0) {
                spinSpeed = 0f;
                isDecelerating = false;
                isSpinning = false;
            }
        }
    }

    void RotateWheels() {
        float directionMultiplier = (rotationDirection == RotationDirection.forward) ? 1f : -1f;
        foreach (Transform wheel in wheels) {
            wheel.Rotate(new Vector3(spinSpeed * directionMultiplier * Time.deltaTime, 0f, 0f));
        }

    }
    public void SlowToStop() {
        if (isSpinning) {
            isDecelerating = true;
        }

    }
    public void StartSpinning() {
        isSpinning = true;
        isDecelerating = false;
    }
}