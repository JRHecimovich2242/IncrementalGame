using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIHoverableTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject tooltip = null;
    private Transform tooltipParent = null;
    private Canvas m_Canvas = null;

    private void Awake()
    {
        if(tooltip != null)
        {
            tooltipParent = tooltip.transform.parent;
        }

        m_Canvas = GetComponentInParent<Canvas>();
    }

    private void OnDestroy()
    {
        if(tooltip != null)
        {
            Destroy(tooltip);
        }
    }

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

            if(!active && tooltipParent != null)
            {
                tooltip.transform.SetParent(tooltipParent, true);
            }
            else if(active && m_Canvas != null)
            {
                tooltip.transform.SetParent(m_Canvas.transform, true);
            }
        }
    }
}
