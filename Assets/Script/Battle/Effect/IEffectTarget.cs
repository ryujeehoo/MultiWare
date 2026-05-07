using System.Collections.Generic;

public interface IEffectTarget
{
    void TakeDamage(float damage);
    void ApplyEffect(StatusEffect effect);
    void TickEffects();
}