using UnityEngine;
using System.Collections.Generic;

public class PlayerEffectDisplay : MonoBehaviour
{
    public static PlayerEffectDisplay Instance;

    [Header("슬롯 프리팹 & 부모")]
    public GameObject slotPrefab;
    public Transform slotParent;

    [Header("개별 상태이상 아이콘")]
    public Sprite iconPoison;
    public Sprite iconShock;
    public Sprite iconBind;

    [Header("버프 아이콘")]
    public Sprite iconFocus;
    public Sprite iconInvincible;

    private List<StatusEffectSlot> slots = new List<StatusEffectSlot>();

    void Awake()
    {
        Instance = this;
    }

    public void Refresh()
    {
        foreach (var slot in slots)
            slot.Hide();

        int index = 0;

        // 개별 상태이상 (독, 감전, 속박)
        if (Player.Instance != null)
        {
            foreach (var effect in Player.Instance.activeEffects)
            {
                if (effect.IsExpired()) continue;

                Sprite icon = null;
                int stack = 0;

                if (effect is PoisonEffect p) { icon = iconPoison; stack = p.stack; }
                if (effect is ShockEffect s) { icon = iconShock; stack = s.stack; }
                if (effect is BindEffect b) { icon = iconBind; stack = b.stack; }

                ShowSlot(ref index, icon, stack);
            }
        }

        // 버프 (집중, 무적)
        var bm = BattleManager.Instance;
        if (bm != null)
        {
            if (bm.isFocused) ShowSlot(ref index, iconFocus, 0);
            if (bm.isInvincible) ShowSlot(ref index, iconInvincible, 0);
        }
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
}