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

    [Header("전체 디버프 아이콘")]
    public Sprite iconWeaken;
    public Sprite iconExpose;
    public Sprite iconPanic;
    public Sprite iconSlow;

    [Header("스택 아이콘")]
    public Sprite iconStack; // ← 추가

    private List<StatusEffectSlot> slots = new List<StatusEffectSlot>();
    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    private int currentStack = 0; // ← 추가

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        Debug.Log($"[StatusEffectDisplay] OnEnable 호출됨 - {gameObject.name}");
        Refresh();
    }

    public void SetStack(int stack) // ← 추가
    {
        currentStack = stack;
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
            if (effect is PoisonEffect p) { icon = iconPoison; stack = p.stack; }
            if (effect is ReflectEffect) { icon = iconReflect; stack = 0; }
            else continue;
            ShowSlot(ref index, icon, stack);
        }

        var bm = BattleManager.Instance;
        if (bm != null)
        {
            Debug.Log($"[StatusEffectDisplay] BM 디버프 - 약화: {bm.isWeakened}, 노출: {bm.isExposed}");
            if (bm.isWeakened) ShowSlot(ref index, iconWeaken, bm.weakenedTurns);
            if (bm.isExposed) ShowSlot(ref index, iconExpose, bm.exposedTurns);
            if (bm.isPanicked) ShowSlot(ref index, iconPanic, 1);
            if (bm.isSlowed) ShowSlot(ref index, iconSlow, 1);
        }

        // ← 스택 슬롯 추가
        if (currentStack > 0)
            ShowSlot(ref index, iconStack, currentStack);
    }

    void ShowSlot(ref int index, Sprite icon, int stack)
    {
        if (index >= slots.Count)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            slots.Add(obj.GetComponent<StatusEffectSlot>());
        }
        slots[index].Setup(icon, stack);
        index++;
    }

    public static void RefreshAll()
    {
        StatusEffectDisplay[] displays = FindObjectsByType<StatusEffectDisplay>(FindObjectsSortMode.None);
        foreach (var display in displays)
            display.Refresh();
    }
}