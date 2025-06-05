using UnityEngine;

public class ArcadeTrigger : MonoBehaviour
{
    [SerializeField] ArcadeController arcadeController;
    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.name == "Hiker") {
            switch (gameObject.name) {
                case "Level1End":
                        arcadeController.HandleLevelTransition(1);
                    break;
                case "Level2End":
                        arcadeController.HandleLevelTransition(2);
                    break;
            }
        }
    }
}
