using UnityEngine;

public class ReflectEffect : StatusEffect
{
    public int reflectDamage;
    private bool firstTurnEnd = true;   // 부여된 턴은 건너뜀
    private bool expiredNextTurn = false;

    public ReflectEffect(int reflectDamage)
    {
        effectName = "반사";
        this.reflectDamage = reflectDamage;
    }

    public override void OnTurnEnd(IEffectTarget target)
    {
        if (firstTurnEnd)
        {
            firstTurnEnd = false;  // 부여된 턴 건너뜀
            Debug.Log($"[반사] 첫 턴 종료 - 유지");
            return;
        }

        expiredNextTurn = true;
        Debug.Log($"[반사] 효과 만료 예약");
    }

    public override bool IsExpired() => expiredNextTurn;
}