using UnityEngine;

public class ReflectEffect : StatusEffect
{
    public int reflectDamage;
    private bool expiredNextTurn = false;

    public ReflectEffect(int reflectDamage)
    {
        effectName = "반사";
        this.reflectDamage = reflectDamage;
    }

    public override void OnTurnEnd(IEffectTarget target)
    {
        expiredNextTurn = true; // 턴 종료 시 만료 예약
        Debug.Log($"[반사] 효과 만료 예약");
    }

    public override bool IsExpired() => expiredNextTurn;
}