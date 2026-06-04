using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterIntentUI : MonoBehaviour
{
    public Image intentIcon;
    public TextMeshProUGUI intentText;
    public TooltipTrigger intentTooltip;
    public Sprite iconAttack;
    public Sprite iconDefend;
    public Sprite iconPoison;
    public Sprite iconMultiAttack;
    public Sprite iconSummon;
    public Sprite iconHeal;
    public Sprite iconBind;
    public Sprite iconShock;
    public Sprite iconReflect;
    public Sprite iconBuff;
    public Sprite iconSelfDestruct;
    public Sprite iconStack;
    public Sprite iconLurk;
    public Sprite iconUnknown;
    public Sprite iconAllyDefend;

    public void UpdateIntent(MonsterAction action, int turnCount, float attackBonus, int actionStack = 0)
    {
        if (action == null)
        {
            intentIcon.sprite = iconUnknown;
            intentText.text = "?";
            intentTooltip?.SetTooltip("알 수 없음", "다음 행동을 알 수 없습니다.");
            return;
        }

        float scale = GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        float weakenMultiplier = (BattleManager.Instance != null && BattleManager.Instance.isWeakened) ? 0.8f : 1f;

        string tooltipTitle = "";
        string tooltipDesc = "";

        switch (action.actionType)
        {
            case ActionType.Attack:
                intentIcon.sprite = iconAttack;
                int atkDmg = Mathf.RoundToInt((action.GetValue(turnCount) + attackBonus) * scale * weakenMultiplier);
                intentText.text = $"공격 {atkDmg}";
                tooltipTitle = "공격";
                tooltipDesc = $"플레이어에게 {atkDmg}의 피해를 입힙니다.";
                break;

            case ActionType.MultiAttack:
                intentIcon.sprite = iconMultiAttack;
                int mDmg = Mathf.RoundToInt((action.GetValue(turnCount) + attackBonus) * scale * weakenMultiplier);
                intentText.text = $"연속 {mDmg}x{action.attackCount}";
                tooltipTitle = "연속 공격";
                tooltipDesc = $"{mDmg}의 피해를 {action.attackCount}회 입힙니다.\n총 피해: {mDmg * action.attackCount}";
                break;

            case ActionType.AttackAndEffect:
                if (action.attackEffectType == AttackEffectType.Poison)
                    intentIcon.sprite = iconPoison;
                else if (action.attackEffectType == AttackEffectType.Shock)
                    intentIcon.sprite = iconShock;
                else
                    intentIcon.sprite = iconAttack;
                int aeDmg = Mathf.RoundToInt(action.GetValue(turnCount) * scale * weakenMultiplier);
                string effectName = action.attackEffectType == AttackEffectType.Poison ? "독" : "감전";
                intentText.text = $"공격 {aeDmg}+{effectName}";
                tooltipTitle = $"공격 + {effectName}";
                tooltipDesc = $"{aeDmg}의 피해를 입히고 {effectName}을 부여합니다.";
                break;

            case ActionType.StackAttack:
                intentIcon.sprite = iconAttack;
                int dmgPerStack = Mathf.RoundToInt(action.GetValue(turnCount) * scale * weakenMultiplier);
                int stackDmg = dmgPerStack * actionStack;
                intentText.text = $"{dmgPerStack}×{actionStack}={stackDmg}";
                tooltipTitle = "스택 공격";
                tooltipDesc = $"현재 스택({actionStack})에 비례하여 피해가 증가합니다.\n총 피해: {stackDmg}";
                break;

            case ActionType.SelfDestruct:
                intentIcon.sprite = iconSelfDestruct;
                int sdDmg = Mathf.RoundToInt(action.GetValue(turnCount) * scale * weakenMultiplier);
                intentText.text = $"폭발 {sdDmg}";
                tooltipTitle = "자폭";
                tooltipDesc = $"{sdDmg}의 피해를 입히고 사망합니다.";
                break;

            case ActionType.Defend:
                intentIcon.sprite = iconDefend;
                intentText.text = $"방어 {(int)action.GetValue(turnCount)}";
                tooltipTitle = "방어";
                tooltipDesc = $"{(int)action.GetValue(turnCount)}의 블록을 획득합니다.";
                break;

            case ActionType.Reflect:
                intentIcon.sprite = iconReflect;
                intentText.text = $"반사 {action.reflectDamage}";
                tooltipTitle = "반사";
                tooltipDesc = $"블록을 획득하고 받은 피해의 일부를 반사합니다.\n반사 피해: {action.reflectDamage}";
                break;

            case ActionType.AllyDefend:
                intentIcon.sprite = iconAllyDefend != null ? iconAllyDefend : iconDefend;
                intentText.text = $"아군방어 {(int)action.GetValue(turnCount)}";
                tooltipTitle = "아군 방어";
                tooltipDesc = $"모든 아군에게 {(int)action.GetValue(turnCount)}의 블록을 부여합니다.";
                break;

            case ActionType.Poison:
                intentIcon.sprite = iconPoison;
                intentText.text = $"독 {action.effectDuration}턴";
                tooltipTitle = "독";
                tooltipDesc = $"플레이어에게 {action.effectDuration}턴 동안 독을 부여합니다.\n매 턴 HP가 감소합니다.";
                break;

            case ActionType.Shock:
                intentIcon.sprite = iconShock;
                intentText.text = $"감전 {action.effectDuration}턴";
                tooltipTitle = "감전";
                tooltipDesc = $"플레이어에게 {action.effectDuration}턴 동안 감전을 부여합니다.\n두 번째 공격부터 높은 카드를 강제합니다.";
                break;

            case ActionType.Bind:
                intentIcon.sprite = iconBind;
                intentText.text = $"속박 {(int)action.GetValue(turnCount)}턴";
                tooltipTitle = "속박";
                tooltipDesc = $"{(int)action.GetValue(turnCount)}턴 동안 드로우가 1 감소합니다.";
                break;

            case ActionType.Heal:
                intentIcon.sprite = iconHeal;
                intentText.text = $"회복 {(int)action.GetValue(turnCount)}";
                tooltipTitle = "회복";
                tooltipDesc = $"HP를 {(int)action.GetValue(turnCount)} 회복합니다.";
                break;

            case ActionType.Summon:
                intentIcon.sprite = iconSummon;
                intentText.text = $"소환 x{(int)action.GetValue(turnCount)}";
                tooltipTitle = "소환";
                tooltipDesc = $"몬스터를 {(int)action.GetValue(turnCount)}마리 소환합니다.";
                break;

            case ActionType.AttackBuff:
                intentIcon.sprite = iconBuff;
                intentText.text = $"공격력 +{(int)action.GetValue(turnCount)}";
                tooltipTitle = "공격력 강화";
                tooltipDesc = $"공격력이 {(int)action.GetValue(turnCount)} 영구적으로 증가합니다.";
                break;

            case ActionType.StackUp:
                intentIcon.sprite = iconStack;
                intentText.text = $"스택 +{(int)action.GetValue(turnCount)}";
                tooltipTitle = "스택 증가";
                tooltipDesc = $"스택이 {(int)action.GetValue(turnCount)} 증가합니다.\n스택이 높을수록 다음 공격이 강해집니다.";
                break;

            case ActionType.Lurk:
                intentIcon.sprite = iconLurk;
                intentText.text = "잠복";
                tooltipTitle = "잠복";
                tooltipDesc = "이번 턴에 받는 모든 피해와 상태이상을 무효화합니다.";
                break;

            default:
                intentIcon.sprite = iconUnknown;
                intentText.text = "?";
                tooltipTitle = "알 수 없음";
                tooltipDesc = "다음 행동을 알 수 없습니다.";
                break;
        }

        intentTooltip?.SetTooltip(tooltipTitle, tooltipDesc);
    }
}