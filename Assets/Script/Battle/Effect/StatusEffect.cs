public abstract class StatusEffect
{
    public string effectName;

    public abstract void OnTurnEnd(IEffectTarget target);
    public abstract bool IsExpired();
}