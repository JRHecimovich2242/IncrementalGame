using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverableTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject tooltip = null;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetTooltipActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetTooltipActive(false);
    }

    protected virtual void SetTooltipActive(bool active)
    {
        if(tooltip != null)
        {
            tooltip.SetActive(active);
        }
    }
}
