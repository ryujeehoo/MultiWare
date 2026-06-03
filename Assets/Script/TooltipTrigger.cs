using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string title;
    [TextArea] public string description;

    private TooltipUI tooltip;

    void Start()
    {
        tooltip = TooltipUI.Instance;
        if (tooltip == null)
            tooltip = FindFirstObjectByType<TooltipUI>();
        if (tooltip == null)
            Debug.LogError("TooltipUI를 찾을 수 없습니다!");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter 호출됨");
        if (tooltip == null)
            Debug.LogError("TooltipUI.Instance가 null!");
        tooltip.Show(title, description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.Hide();
    }

    public void SetTooltip(string newTitle, string newDesc)
    {
        title = newTitle;
        description = newDesc;
    }
}