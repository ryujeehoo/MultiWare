using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterIntentUI : MonoBehaviour
{
    public Image intentIcon;
    public TextMeshProUGUI intentText;

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

    // UpdateIntent 시그니처에 actionStack 추가
    public void UpdateIntent(MonsterAction action, int turnCount, float attackBonus, int actionStack = 0)
    {
        if (action == null)
        {
            intentIcon.sprite = iconUnknown;
            intentText.text = "?";
            return;
        }

        float scale = GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        float weakenMultiplier = (BattleManager.Instance != null && BattleManager.Instance.isWeakened) ? 0.8f : 1f;

        switch (action.actionType)
        {
            case ActionType.Attack:
                intentIcon.sprite = iconAttack;
                int atkDmg = Mathf.RoundToInt((action.GetValue(turnCount) + attackBonus) * scale * weakenMultiplier);
                intentText.text = $"Attack {atkDmg}";
                break;

            case ActionType.MultiAttack:
                intentIcon.sprite = iconMultiAttack;
                int mDmg = Mathf.RoundToInt((action.GetValue(turnCount) + attackBonus) * scale * weakenMultiplier);
                intentText.text = $"Multi {mDmg}x{action.attackCount}";
                break;

            case ActionType.AttackAndEffect:
                intentIcon.sprite = iconAttack;
                int aeDmg = Mathf.RoundToInt(action.GetValue(turnCount) * scale * weakenMultiplier);
                string effectName = action.attackEffectType == AttackEffectType.Poison ? "Poison" : "Shock";
                intentText.text = $"Attack {aeDmg} + {effectName}";
                break;

            case ActionType.StackAttack:
                intentIcon.sprite = iconAttack;
                int dmgPerStack = Mathf.RoundToInt(action.GetValue(turnCount) * scale * weakenMultiplier);
                int stackDmg = dmgPerStack * actionStack;
                intentText.text = $"{dmgPerStack}×{actionStack} = {stackDmg}";
                break;
            case ActionType.SelfDestruct:
                intentIcon.sprite = iconSelfDestruct;
                int sdDmg = Mathf.RoundToInt(action.GetValue(turnCount) * scale * weakenMultiplier);
                intentText.text = $"Explode {sdDmg}";
                break;

            // 나머지는 그대로
            case ActionType.Defend:
                intentIcon.sprite = iconDefend;
                intentText.text = $"Block {(int)action.GetValue(turnCount)}";
                break;

            case ActionType.Reflect:
                intentIcon.sprite = iconReflect;
                intentText.text = $"Reflect {action.reflectDamage}";
                break;

            case ActionType.AllyDefend:
                intentIcon.sprite = iconDefend;
                intentText.text = $"Ally Block {(int)action.GetValue(turnCount)}";
                break;

            case ActionType.Poison:
                intentIcon.sprite = iconPoison;
                intentText.text = $"Poison {action.effectDuration}t";
                break;

            case ActionType.Shock:
                intentIcon.sprite = iconShock;
                intentText.text = $"Shock {action.effectDuration}t";
                break;

            case ActionType.Bind:
                intentIcon.sprite = iconBind;
                intentText.text = $"Bind {(int)action.GetValue(turnCount)}t";
                break;

            case ActionType.Heal:
                intentIcon.sprite = iconHeal;
                intentText.text = $"Heal {(int)action.GetValue(turnCount)}";
                break;

            case ActionType.Summon:
                intentIcon.sprite = iconSummon;
                intentText.text = $"Summon x{(int)action.GetValue(turnCount)}";
                break;

            case ActionType.AttackBuff:
                intentIcon.sprite = iconBuff;
                intentText.text = $"ATK +{(int)action.GetValue(turnCount)}";
                break;

            case ActionType.StackUp:
                intentIcon.sprite = iconStack;
                intentText.text = $"Stack +{(int)action.GetValue(turnCount)}";
                break;
            case ActionType.Lurk:
                intentIcon.sprite = iconLurk;
                intentText.text = "Lurk";
                break;
            default:
                intentIcon.sprite = iconUnknown;
                intentText.text = "?";
                break;
        }
    }
}