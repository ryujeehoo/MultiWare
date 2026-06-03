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
            intentTooltip?.SetTooltip("Unknown", "The next action is unknown.");
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
                intentText.text = $"Attack {atkDmg}";
                tooltipTitle = "Attack";
                tooltipDesc = $"Deals {atkDmg} damage to the player.";
                break;

            case ActionType.MultiAttack:
                intentIcon.sprite = iconMultiAttack;
                int mDmg = Mathf.RoundToInt((action.GetValue(turnCount) + attackBonus) * scale * weakenMultiplier);
                intentText.text = $"Multi {mDmg}x{action.attackCount}";
                tooltipTitle = "Multi-Attack";
                tooltipDesc = $"Deals {mDmg} damage {action.attackCount} times.\nTotal: {mDmg * action.attackCount} damage";
                break;

            case ActionType.AttackAndEffect:
                if (action.attackEffectType == AttackEffectType.Poison)
                    intentIcon.sprite = iconPoison;
                else if (action.attackEffectType == AttackEffectType.Shock)
                    intentIcon.sprite = iconShock;
                else
                    intentIcon.sprite = iconAttack;
                int aeDmg = Mathf.RoundToInt(action.GetValue(turnCount) * scale * weakenMultiplier);
                string effectName = action.attackEffectType == AttackEffectType.Poison ? "Poison" : "Shock";
                intentText.text = $"Atk {aeDmg}+{effectName}";
                tooltipTitle = $"Attack + {effectName}";
                tooltipDesc = $"Deals {aeDmg} damage and inflicts {effectName}.";
                break;

            case ActionType.StackAttack:
                intentIcon.sprite = iconAttack;
                int dmgPerStack = Mathf.RoundToInt(action.GetValue(turnCount) * scale * weakenMultiplier);
                int stackDmg = dmgPerStack * actionStack;
                intentText.text = $"{dmgPerStack}¡¿{actionStack}={stackDmg}";
                tooltipTitle = "Stack Attack";
                tooltipDesc = $"Damage increases based on current stack ({actionStack}).\nTotal: {stackDmg} damage";
                break;

            case ActionType.SelfDestruct:
                intentIcon.sprite = iconSelfDestruct;
                int sdDmg = Mathf.RoundToInt(action.GetValue(turnCount) * scale * weakenMultiplier);
                intentText.text = $"Explode {sdDmg}";
                tooltipTitle = "Self-Destruct";
                tooltipDesc = $"Deals {sdDmg} damage and dies.";
                break;

            case ActionType.Defend:
                intentIcon.sprite = iconDefend;
                intentText.text = $"Block {(int)action.GetValue(turnCount)}";
                tooltipTitle = "Defend";
                tooltipDesc = $"Gains {(int)action.GetValue(turnCount)} block.";
                break;

            case ActionType.Reflect:
                intentIcon.sprite = iconReflect;
                intentText.text = $"Reflect {action.reflectDamage}";
                tooltipTitle = "Reflect";
                tooltipDesc = $"Gains block and reflects a portion of received damage.\nReflect damage: {action.reflectDamage}";
                break;

            case ActionType.AllyDefend:
                intentIcon.sprite = iconAllyDefend != null ? iconAllyDefend : iconDefend;
                intentText.text = $"AllyBlk {(int)action.GetValue(turnCount)}";
                tooltipTitle = "Ally Defend";
                tooltipDesc = $"Grants {(int)action.GetValue(turnCount)} block to all allies.";
                break;

            case ActionType.Poison:
                intentIcon.sprite = iconPoison;
                intentText.text = $"Poison {action.effectDuration}t";
                tooltipTitle = "Poison";
                tooltipDesc = $"Inflicts Poison on the player for {action.effectDuration} turns.\nHP decreases each turn.";
                break;

            case ActionType.Shock:
                intentIcon.sprite = iconShock;
                intentText.text = $"Shock {action.effectDuration}t";
                tooltipTitle = "Shock";
                tooltipDesc = $"Inflicts Shock on the player for {action.effectDuration} turns.\nForces High Card from the second attack onward.";
                break;

            case ActionType.Bind:
                intentIcon.sprite = iconBind;
                intentText.text = $"Bind {(int)action.GetValue(turnCount)}t";
                tooltipTitle = "Bind";
                tooltipDesc = $"Reduces draw by 1 for {(int)action.GetValue(turnCount)} turns.";
                break;

            case ActionType.Heal:
                intentIcon.sprite = iconHeal;
                intentText.text = $"Heal {(int)action.GetValue(turnCount)}";
                tooltipTitle = "Heal";
                tooltipDesc = $"Restores {(int)action.GetValue(turnCount)} HP.";
                break;

            case ActionType.Summon:
                intentIcon.sprite = iconSummon;
                intentText.text = $"Summon x{(int)action.GetValue(turnCount)}";
                tooltipTitle = "Summon";
                tooltipDesc = $"Summons {(int)action.GetValue(turnCount)} monster(s).";
                break;

            case ActionType.AttackBuff:
                intentIcon.sprite = iconBuff;
                intentText.text = $"ATK +{(int)action.GetValue(turnCount)}";
                tooltipTitle = "Attack Buff";
                tooltipDesc = $"Permanently increases attack by {(int)action.GetValue(turnCount)}.";
                break;

            case ActionType.StackUp:
                intentIcon.sprite = iconStack;
                intentText.text = $"Stack +{(int)action.GetValue(turnCount)}";
                tooltipTitle = "Stack Up";
                tooltipDesc = $"Increases stack by {(int)action.GetValue(turnCount)}.\nHigher stack strengthens the next attack.";
                break;

            case ActionType.Lurk:
                intentIcon.sprite = iconLurk;
                intentText.text = "Lurk";
                tooltipTitle = "Lurk";
                tooltipDesc = "Negates all damage and status effects this turn.";
                break;

            default:
                intentIcon.sprite = iconUnknown;
                intentText.text = "?";
                tooltipTitle = "Unknown";
                tooltipDesc = "The next action is unknown.";
                break;
        }

        intentTooltip?.SetTooltip(tooltipTitle, tooltipDesc);
    }
}