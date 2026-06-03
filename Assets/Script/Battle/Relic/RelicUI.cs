using UnityEngine;
using UnityEngine.UI;

public class RelicUI : MonoBehaviour
{
    public Image icon;
    private TooltipTrigger tooltip;

    void Awake()
    {
        tooltip = GetComponent<TooltipTrigger>();
        if (tooltip == null)
            tooltip = gameObject.AddComponent<TooltipTrigger>();
    }

    public void Setup(Relic relic)
    {
        if (relic.icon != null)
            icon.sprite = relic.icon;

        tooltip.SetTooltip(relic.relicName, relic.description);
    }
}