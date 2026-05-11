using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Relic/GamblerDice")]
public class GamblerDiceRelic : Relic
{
    public override void OnAttack(Player player, List<CardData> usedCards, Monster target, HandResult handResult)
    {
        if (Random.value <= 0.5f)
        {
            BattleManager.Instance.AddDamageMultiplier(1.0f); // ×2
            Debug.Log($"[{relicName}] 대박! 데미지 ×2");
        }
        else
        {
            BattleManager.Instance.AddDamageMultiplier(-0.5f); // ×0.5
            Debug.Log($"[{relicName}] 꽝! 데미지 ×0.5");
        }
    }
}