using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    public GameObject tooltipPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;

    private RectTransform panelRect;
    private Canvas canvas;

    void Awake()
    {
        Instance = this;
        panelRect = tooltipPanel.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        tooltipPanel.SetActive(false);
        Debug.Log("TooltipUI Awake ¿Ï·á");
    }

    void Update()
    {
        if (!tooltipPanel.activeSelf) return;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            Input.mousePosition,
            canvas.worldCamera,
            out pos
        );

        panelRect.localPosition = pos + new Vector2(100f, -100f);
    }

    public void Show(string title, string desc)
    {
        Debug.Log($"Show È£ÃâµÊ - title: {title}, desc: {desc}");
        titleText.text = title;
        descText.text = desc;
        tooltipPanel.SetActive(true);
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }
}