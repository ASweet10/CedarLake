using System;
using UnityEngine;

public class Axe : MonoBehaviour
{
    FirstPersonHealth fpHealth;
    void Start() {
        fpHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonHealth>();
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Player") {
            fpHealth.HandleTakeDamage();
        }
    }
}
