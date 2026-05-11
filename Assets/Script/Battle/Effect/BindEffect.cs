using UnityEngine;

public class BindEffect : StatusEffect
{
    public int stack;
    public int drawReduction;

    public BindEffect(int stack, int drawReduction = 1)
    {
        effectName = "속박";
        this.stack = stack;
        this.drawReduction = drawReduction;
    }

    public override void OnTurnEnd(IEffectTarget target)
    {
        stack--;
        Debug.Log($"[속박] 남은 턴: {stack}");
    }

    public override bool IsExpired() => stack <= 0;
}