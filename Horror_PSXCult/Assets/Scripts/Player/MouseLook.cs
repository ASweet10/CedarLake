using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attached to camera, which is child of player for first-person pov
public class MouseLook : MonoBehaviour
{
    [Header("Mouse Look")]
    [SerializeField]  float mouseSensitivity = 100f;
    [SerializeField]  Transform playerBody;
    float xRotation = 0f;

    bool canRotateMouse = true;
    public bool CanRotateMouse {
        get { return canRotateMouse; }
        set { canRotateMouse = value; }
    }

    void Update() {
        if(canRotateMouse) {
            HandleMouseLook();
        }
    }

    void HandleMouseLook() {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -65f, 65f); //  Clamp camera rotation so you can't look past a certain point 

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}