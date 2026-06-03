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
        if (Player.Instance != null)
        {
            foreach (var effect in Player.Instance.activeEffects)
            {
                if (effect.IsExpired()) continue;
                Sprite icon = null;
                int stack = 0;
                string title = "", desc = "";
                if (effect is PoisonEffect p)
                {
                    icon = iconPoison; stack = p.stack;
                    title = "독";
                    desc = $"턴 종료 시 {p.stack}의 피해를 받습니다.";
                }
                else if (effect is ShockEffect s)
                {
                    icon = iconShock; stack = s.stack;
                    title = "감전";
                    desc = $"족보가 하이카드로 강제됩니다. ({s.stack}턴 남음)";
                }
                else if (effect is BindEffect b)
                {
                    icon = iconBind; stack = b.stack;
                    title = "속박";
                    desc = $"행동이 제한됩니다. ({b.stack}턴 남음)";
                }
                ShowSlot(ref index, icon, stack, title, desc);
            }
        }
        var bm = BattleManager.Instance;
        if (bm != null)
        {
            if (bm.isFocused)
                ShowSlot(ref index, iconFocus, 0,
                    "집중", "다음 족보 데미지가 +40% 증가합니다.");
            if (bm.isInvincible)
                ShowSlot(ref index, iconInvincible, 0,
                    "무적", "이번 턴 받는 피해가 0이 됩니다.");
        }
    }
    void ShowSlot(ref int index, Sprite icon, int stack, string title = "", string desc = "")
    {
        if (index >= slots.Count)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            slots.Add(obj.GetComponent<StatusEffectSlot>());
        }
        slots[index].Setup(icon, stack);

        // 툴팁 연결
        if (!string.IsNullOrEmpty(title))
        {
            TooltipTrigger trigger = slots[index].GetComponent<TooltipTrigger>();
            if (trigger == null)
                trigger = slots[index].gameObject.AddComponent<TooltipTrigger>();
            trigger.SetTooltip(title, desc);
        }

        index++;
    }
}