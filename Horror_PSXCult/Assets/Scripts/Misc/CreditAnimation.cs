using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditAnimation : MonoBehaviour
{
    GameController gameController;
    void Awake() {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }
    public void FinishCreditScroll() {
        gameController.ReturnToMainMenu();
    }
}
