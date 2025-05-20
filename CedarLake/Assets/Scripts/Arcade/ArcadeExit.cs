using UnityEngine;

public class ArcadeExit : MonoBehaviour
{
    [SerializeField] ArcadeController arcadeController;

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.name == "Exit") {
            arcadeController.HandleWinGame();
        }
    }
}