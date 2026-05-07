using UnityEngine;
public class PoisonEffect : StatusEffect
{
    public int stack; // 중첩 수

    public PoisonEffect(int stack) : base()
    {
        effectName = "독";
        this.stack = stack;
    }

    public override void OnTurnEnd(IEffectTarget target)
    {
        target.TakeDamage(stack); // 중첩 수만큼 데미지
        stack--;                  // 턴마다 1중첩 감소
        Debug.Log($"[독] {stack + 1} 데미지! 남은 중첩: {stack}");
    }

    public override bool IsExpired() => stack <= 0;
}