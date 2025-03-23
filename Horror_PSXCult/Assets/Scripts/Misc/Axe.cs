using System;
using UnityEngine;

public class Axe : MonoBehaviour
{
    FirstPersonController fpController;
    void Start() {
        fpController = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Player") {
            fpController.TakeAxeDamage();
        }
    }
}
