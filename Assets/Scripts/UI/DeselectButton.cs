using UnityEngine;
using UnityEngine.EventSystems;

public class DeselectButton : MonoBehaviour, IPointerExitHandler
{
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
}
