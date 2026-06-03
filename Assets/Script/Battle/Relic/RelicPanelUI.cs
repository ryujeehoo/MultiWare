using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RelicPanelUI : MonoBehaviour
{
    public static RelicPanelUI Instance;

    [Header("UI")]
    public GameObject panel;
    public Transform slotParent;
    public GameObject slotPrefab;
    public Button toggleButton;

    private List<RelicUI> slots = new List<RelicUI>();
    public bool isOpen = false;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    void Start()
    {
        //toggleButton.onClick.AddListener(Toggle);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        Debug.Log($"Toggle 호출됨 - isOpen: {isOpen}");
        panel.SetActive(isOpen);

        if (isOpen)
            Refresh();
    }

    public void Refresh()
    {
        foreach (var slot in slots)
            Destroy(slot.gameObject);
        slots.Clear();

        if (Player.Instance == null) return;

        if (Player.Instance.relics.Count == 0)
        {
            // 유물 없을 때 처리는 필요하면 말씀해주세요
            return;
        }

        foreach (var relic in Player.Instance.relics)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            RelicUI slot = obj.GetComponent<RelicUI>();
            slot.Setup(relic);
            slots.Add(slot);
        }
    }
}