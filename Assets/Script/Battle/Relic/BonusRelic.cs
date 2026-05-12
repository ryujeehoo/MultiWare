using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Relic/BonusRelic")]
public class BonusRelic : Relic
{
    public enum TriggerType
    {
        OnAcquired,     // 유물 획득 시
        OnBattleStart,  // 전투 시작 시
        OnTurnStart,    // 턴 시작 시
        OnAttack,       // 공격 시
        OnTakeDamage,   // 피해 수신 시
        OnBattleEnd     // 전투 종료 시
    }

    public enum ConditionType
    {
        None,           // 조건 없음
        HandRank,       // 족보 조건
        Suit,           // 무늬 조건
        CardNumber,     // 숫자 조건
        TurnInterval,   // n턴마다
        HpBelow,        // HP n% 이하
    }

    public enum EffectType
    {
        BonusDamageMultiplier,  // 데미지 배율
        BonusHandRankMultiplier,// 족보 배율
        BonusGold,              // 골드 획득
        SpendGold,              // 골드 소모
        BonusHeal,              // HP 회복
        BonusDraw,              // 드로우 추가
        MaxHpChange,            // 최대 HP 변경
        GoldMultiplier,         // 골드 배율
        HealMultiplier,         // 회복 배율
        AddDefense,             // 방어막
        HeartHealMultiplier,    // 하트 회복 배율
        DiamondGoldMultiplier,  // 다이아 골드 배율
        DebuffDurationBonus, // 디버프 지속 턴 +n
    }

    [System.Serializable]
    public class RelicEffect
    {
        public EffectType effectType;
        public float effectValue;
    }

    [System.Serializable]
    public class RelicTrigger
    {
        [Header("트리거")]
        public TriggerType triggerType;

        [Header("조건")]
        public ConditionType conditionType;

        // 족보 조건
        public List<HandRankType> triggerRanks = new List<HandRankType>();

        // 무늬 조건
        public SuitType triggerSuit;

        // 숫자 조건
        public List<int> triggerNumbers = new List<int>();

        // n턴마다 조건
        public int turnInterval = 3;

        // HP 이하 조건
        public float hpThreshold = 0.5f; // 0.5 = 50%

        [Header("효과 목록")]
        public List<RelicEffect> effects = new List<RelicEffect>();
    }

    [Header("트리거 목록")]
    public List<RelicTrigger> triggers = new List<RelicTrigger>();

    // 조건 체크
    bool CheckCondition(RelicTrigger trigger, List<CardData> usedCards, HandResult handResult, Player player)
    {
        switch (trigger.conditionType)
        {
            case ConditionType.None:
                return true;

            case ConditionType.HandRank:
                return handResult != null && trigger.triggerRanks.Contains(handResult.rank);

            case ConditionType.Suit:
                return usedCards != null && usedCards.Exists(card => card.suit == trigger.triggerSuit);

            case ConditionType.CardNumber:
                return usedCards != null && usedCards.Exists(card => trigger.triggerNumbers.Contains(card.cardNumber));

            case ConditionType.TurnInterval:
                return TurnManager.Instance.turnCount % trigger.turnInterval == 0;

            case ConditionType.HpBelow:
                return player.currentHealth <= player.maxHealth * trigger.hpThreshold;

            default:
                return false;
        }
    }

    // 효과 적용
    void ApplyEffects(RelicTrigger trigger, Player player, Monster target = null)
    {
        foreach (var effect in trigger.effects)
        {
            switch (effect.effectType)
            {
                case EffectType.BonusDamageMultiplier:
                    BattleManager.Instance.AddDamageMultiplier(effect.effectValue);
                    Debug.Log($"[{relicName}] 데미지 배율 +{effect.effectValue}");
                    break;

                case EffectType.BonusHandRankMultiplier:
                    BattleManager.Instance.AddHandRankMultiplier(effect.effectValue);
                    Debug.Log($"[{relicName}] 족보 배율 +{effect.effectValue}");
                    break;

                case EffectType.BonusGold:
                    GoldManager.Instance.AddGold((int)effect.effectValue);
                    Debug.Log($"[{relicName}] 골드 +{effect.effectValue}");
                    break;

                case EffectType.SpendGold:
                    GoldManager.Instance.SpendGold((int)effect.effectValue);
                    Debug.Log($"[{relicName}] 골드 -{effect.effectValue}");
                    break;

                case EffectType.BonusHeal:
                    player.Heal(effect.effectValue);
                    Debug.Log($"[{relicName}] HP +{effect.effectValue}");
                    break;

                case EffectType.BonusDraw:
                    TurnManager.Instance.bonusDrawCount += (int)effect.effectValue;
                    Debug.Log($"[{relicName}] 드로우 +{effect.effectValue}");
                    break;

                case EffectType.MaxHpChange:
                    player.maxHealth += effect.effectValue;
                    player.currentHealth = Mathf.Min(player.currentHealth, player.maxHealth);
                    Debug.Log($"[{relicName}] 최대 HP {effect.effectValue}");
                    break;

                case EffectType.GoldMultiplier:
                    GoldManager.Instance.AddGoldMultiplier(effect.effectValue);
                    Debug.Log($"[{relicName}] 골드 배율 +{effect.effectValue}");
                    break;

                case EffectType.HealMultiplier:
                    player.AddHealMultiplier(effect.effectValue);
                    Debug.Log($"[{relicName}] 회복 배율 +{effect.effectValue}");
                    break;

                case EffectType.AddDefense:
                    player.AddDefense((int)effect.effectValue);
                    Debug.Log($"[{relicName}] 방어막 +{effect.effectValue}");
                    break;
                case EffectType.HeartHealMultiplier:
                    player.heartHealMultiplier += effect.effectValue;
                    Debug.Log($"[{relicName}] 하트 회복 배율 +{effect.effectValue}");
                    break;

                case EffectType.DiamondGoldMultiplier:
                    GoldManager.Instance.diamondGoldMultiplier += effect.effectValue;
                    Debug.Log($"[{relicName}] 다이아 골드 배율 +{effect.effectValue}");
                    break;
                case EffectType.DebuffDurationBonus:
                    BattleManager.Instance.debuffDurationBonus += (int)effect.effectValue;
                    Debug.Log($"[{relicName}] 디버프 지속 턴 +{effect.effectValue}");
                    break;
            }
        }
    }

    // 트리거별 훅 연결
    public override void OnRelicAcquired(Player player, DeckManager deck)
    {
        foreach (var trigger in triggers)
            if (trigger.triggerType == TriggerType.OnAcquired &&
                CheckCondition(trigger, null, null, player))
                ApplyEffects(trigger, player);
    }

    public override void OnBattleStart(Player player)
    {
        foreach (var trigger in triggers)
            if (trigger.triggerType == TriggerType.OnBattleStart &&
                CheckCondition(trigger, null, null, player))
                ApplyEffects(trigger, player);
    }

    public override void OnTurnStart(Player player)
    {
        foreach (var trigger in triggers)
            if (trigger.triggerType == TriggerType.OnTurnStart &&
                CheckCondition(trigger, null, null, player))
                ApplyEffects(trigger, player);
    }

    public override void OnAttack(Player player, List<CardData> usedCards, Monster target, HandResult handResult)
    {
        foreach (var trigger in triggers)
            if (trigger.triggerType == TriggerType.OnAttack &&
                CheckCondition(trigger, usedCards, handResult, player))
                ApplyEffects(trigger, player, target);
    }

    public override void OnTakeDamage(Player player, float damage)
    {
        foreach (var trigger in triggers)
            if (trigger.triggerType == TriggerType.OnTakeDamage &&
                CheckCondition(trigger, null, null, player))
                ApplyEffects(trigger, player);
    }

    public override void OnBattleEnd(Player player)
    {
        foreach (var trigger in triggers)
            if (trigger.triggerType == TriggerType.OnBattleEnd &&
                CheckCondition(trigger, null, null, player))
                ApplyEffects(trigger, player);
    }
}