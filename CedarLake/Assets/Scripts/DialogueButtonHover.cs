using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image hoverIcon;
    public void OnPointerEnter(PointerEventData eventData) {
        hoverIcon.enabled = true;
    }
    public void OnPointerExit(PointerEventData eventData) {
        hoverIcon.enabled = false;
    }
}