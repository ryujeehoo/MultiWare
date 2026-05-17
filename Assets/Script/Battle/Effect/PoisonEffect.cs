using UnityEngine;

public class PoisonEffect : StatusEffect
{
    public int stack;        // 지속 턴 수
    public int damagePerTurn = 3; // 고정 데미지

    public PoisonEffect(int stack)
    {
        effectName = "독";
        this.stack = stack;
    }

    public override void OnTurnEnd(IEffectTarget target)
    {
        target.TakeDamage(damagePerTurn);
        stack--;
        Debug.Log($"[독] {damagePerTurn} 데미지! 남은 턴: {stack}");
    }

    public override bool IsExpired() => stack <= 0;
}