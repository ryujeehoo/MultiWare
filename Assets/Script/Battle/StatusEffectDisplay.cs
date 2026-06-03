using UnityEngine;
using System.Collections.Generic;

public class StatusEffectDisplay : MonoBehaviour
{
    public static StatusEffectDisplay Instance;

    [Header("슬롯 프리팹 & 부모")]
    public GameObject slotPrefab;
    public Transform slotParent;

    [Header("개별 상태이상 아이콘")]
    public Sprite iconPoison;
    public Sprite iconReflect;
    public Sprite iconLurk;

    [Header("전체 디버프 아이콘")]
    public Sprite iconWeaken;
    public Sprite iconExpose;
    public Sprite iconPanic;
    public Sprite iconSlow;

    [Header("스택 아이콘")]
    public Sprite iconStack;

    private List<StatusEffectSlot> slots = new List<StatusEffectSlot>();
    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    private int currentStack = 0;
    private bool isLurking = false;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        Debug.Log($"[StatusEffectDisplay] OnEnable 호출됨 - {gameObject.name}");
        Refresh();
    }

    public void SetStack(int stack)
    {
        currentStack = stack;
        Refresh();
    }

    public void SetLurk(bool lurking)
    {
        isLurking = lurking;
        Refresh();
    }

    public void Refresh(List<StatusEffect> effects = null)
    {
        if (effects != null)
            activeEffects = effects;

        foreach (var slot in slots)
            slot.Hide();

        int index = 0;

        foreach (var effect in activeEffects)
        {
            if (effect.IsExpired()) continue;

            Sprite icon = null;
            int stack = 0;
            string tooltipTitle = "";
            string tooltipDesc = "";

            if (effect is PoisonEffect p)
            {
                icon = iconPoison;
                stack = p.stack;
                tooltipTitle = "독";
                tooltipDesc = $"매 턴 HP가 감소합니다.\n현재 중첩: {p.stack}";
            }
            else if (effect is ReflectEffect)
            {
                icon = iconReflect;
                stack = 0;
                tooltipTitle = "반사";
                tooltipDesc = "받은 데미지의 일부를 공격자에게 반사합니다.";
            }
            else continue;

            ShowSlot(ref index, icon, stack, tooltipTitle, tooltipDesc);
        }

        var bm = BattleManager.Instance;
        if (bm != null)
        {
            Debug.Log($"[StatusEffectDisplay] BM 디버프 - 약화: {bm.isWeakened}, 노출: {bm.isExposed}");

            if (bm.isWeakened)
                ShowSlot(ref index, iconWeaken, bm.weakenedTurns,
                    "약화", $"공격력이 20% 감소합니다.\n남은 턴: {bm.weakenedTurns}");

            if (bm.isExposed)
                ShowSlot(ref index, iconExpose, bm.exposedTurns,
                    "노출", $"받는 데미지가 20% 증가합니다.\n남은 턴: {bm.exposedTurns}");

            if (bm.isPanicked)
                ShowSlot(ref index, iconPanic, 1,
                    "공황", "이번 턴 랜덤하게 행동합니다.");

            if (bm.isSlowed)
                ShowSlot(ref index, iconSlow, 1,
                    "둔화", "행동이 1턴 지연됩니다.");
        }

        if (isLurking)
            ShowSlot(ref index, iconLurk, 0,
                "잠복", "이번 턴 모든 데미지와 상태이상을 무효화합니다.");

        if (currentStack > 0)
            ShowSlot(ref index, iconStack, currentStack,
                "스택", $"현재 스택: {currentStack}\n스택이 높을수록 다음 공격이 강해집니다.");
    }

    void ShowSlot(ref int index, Sprite icon, int stack, string tooltipTitle = "", string tooltipDesc = "")
    {
        if (index >= slots.Count)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            slots.Add(obj.GetComponent<StatusEffectSlot>());
        }

        slots[index].Setup(icon, stack);

        // 툴팁 설정
        if (!string.IsNullOrEmpty(tooltipTitle))
        {
            TooltipTrigger trigger = slots[index].GetComponent<TooltipTrigger>();
            if (trigger == null)
                trigger = slots[index].gameObject.AddComponent<TooltipTrigger>();
            trigger.SetTooltip(tooltipTitle, tooltipDesc);
        }

        index++;
    }

    public static void RefreshAll()
    {
        StatusEffectDisplay[] displays = FindObjectsByType<StatusEffectDisplay>(FindObjectsSortMode.None);
        foreach (var display in displays)
            display.Refresh();
    }
}