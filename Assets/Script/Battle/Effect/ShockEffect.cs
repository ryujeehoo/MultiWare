using UnityEngine;

public class ShockEffect : StatusEffect
{
    public int stack;
    public int damagePerActivation; // 족보 발동마다 데미지

    public ShockEffect(int stack, int damagePerActivation)
    {
        effectName = "감전";
        this.stack = stack;
        this.damagePerActivation = damagePerActivation;
    }

    // 족보 발동 시 호출
    public void OnHandActivated(IEffectTarget target)
    {
        target.TakeDamage(damagePerActivation);
        Debug.Log($"[감전] 족보 발동! {damagePerActivation} 추가 데미지");
    }

    public override void OnTurnEnd(IEffectTarget target)
    {
        stack--;
        Debug.Log($"[감전] 남은 턴: {stack}");
    }

    public override bool IsExpired() => stack <= 0;
}