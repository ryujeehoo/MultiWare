using UnityEngine;

public class ShockEffect : StatusEffect
{
    public int stack;

    public ShockEffect(int stack)
    {
        effectName = "감전";
        this.stack = stack;
    }

    public override void OnTurnEnd(IEffectTarget target)
    {
        stack--;
        Debug.Log($"[감전] 남은 턴: {stack}");
    }

    public override bool IsExpired() => stack <= 0;
}