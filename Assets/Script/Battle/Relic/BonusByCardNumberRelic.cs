using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Relic/BonusByCardNumber")]
public class BonusByCardNumberRelic : Relic
{
    [Header("발동 숫자 (1=A, 11=J, 12=Q, 13=K)")]
    public List<int> triggerNumbers;

    [Header("효과")]
    public int bonusDamage;
    public int bonusGold;
    public int bonusHeal;

    public override void OnAttack(Player player, List<CardData> usedCards, Monster target, HandResult handResult)
    {
        bool triggered = usedCards.Exists(card => triggerNumbers.Contains(card.cardNumber));
        if (!triggered) return;

        if (bonusDamage > 0)
        {
            target.TakeDamage(bonusDamage);
            Debug.Log($"[{relicName}] 추가 데미지 +{bonusDamage}");
        }

        if (bonusGold > 0)
        {
            GoldManager.Instance.AddGold(bonusGold);
            Debug.Log($"[{relicName}] 골드 +{bonusGold}");
        }

        if (bonusHeal > 0)
        {
            player.Heal(bonusHeal);
            Debug.Log($"[{relicName}] HP +{bonusHeal}");
        }
    }
}