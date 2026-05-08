using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Relic/BonusByHandRank")]
public class BonusByHandRankRelic : Relic
{
    [Header("발동 족보")]
    public List<HandRankType> triggerRanks;

    [Header("효과")]
    public int bonusDamage;
    public int bonusGold;
    public int bonusHeal;

    public override void OnAttack(Player player, List<CardData> usedCards, Monster target, HandResult handResult)
    {
        if (!triggerRanks.Contains(handResult.rank)) return;

        if (bonusDamage > 0)
        {
            target.TakeDamage(bonusDamage);
            Debug.Log($"[{relicName}] {handResult.GetRankName()} 발동! 추가 데미지 {bonusDamage}");
        }

        if (bonusGold > 0)
        {
            GoldManager.Instance.AddGold(bonusGold);
            Debug.Log($"[{relicName}] 골드 {bonusGold} 획득!");
        }

        if (bonusHeal > 0)
        {
            player.Heal(bonusHeal);
            Debug.Log($"[{relicName}] HP +{bonusHeal} 회복!");
        }
    }
}