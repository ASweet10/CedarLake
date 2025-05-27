using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class PauseMenuHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    GameController gameController;
    Interactables interactables;
    public enum itemName {Keys, Drink, LighterFluid, Lighter};
    public itemName ItemName;
    [SerializeField] TMP_Text pauseItemText;

    private void Start() {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        interactables = GameObject.FindGameObjectWithTag("GameController").GetComponent<Interactables>();
    }
    public void OnPointerEnter(PointerEventData eventData) {
        switch (ItemName) {
            case itemName.Keys:
                if(gameController.hasCarKeys) {
                    pauseItemText.text = "The keys to David's car";
                    break;
                }
                break;
            case itemName.Drink:
                if(gameController.hasDrink) {
                    switch(gameController.chosenDrinkIndex) {
                        case 0:
                            pauseItemText.text = "Volt Energy [+10 sprint speed 8 sec]";
                            break;
                        case 1:
                            pauseItemText.text = "Daily Grind [Unlimited stamina 15 sec]";
                            break;
                        case 2:
                            pauseItemText.text = "Noca Cola [+5 sprint speed 15 sec]";
                            break;
                    }
                    break;
                }
                break;
            case itemName.LighterFluid:
                if(gameController.hasLighterFluid) {
                    pauseItemText.text = "A bottle of lighter fluid";
                    break;
                }
                break;
            case itemName.Lighter:
                if(gameController.hasZippo) {
                    pauseItemText.text = "A cheap Zippy lighter";
                    break;
                }
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        pauseItemText.text = "";
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(ItemName == itemName.Drink) {
            if(gameController.hasDrink) {
                interactables.ToggleUseDrinkUI(true);
            }
        }
    }
}