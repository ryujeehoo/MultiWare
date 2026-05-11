using UnityEngine;

public class PoisonEffect : StatusEffect
{
    public int stack;
    public float damageMultiplier = 1f;

    public PoisonEffect(int stack)
    {
        effectName = "독";
        this.stack = stack;
    }

    public override void OnTurnEnd(IEffectTarget target)
    {
        int damage = Mathf.RoundToInt(stack * damageMultiplier);
        target.TakeDamage(damage);
        stack--;
        Debug.Log($"[독] {damage} 데미지! 남은 중첩: {stack}");
    }

    public override bool IsExpired() => stack <= 0;
}