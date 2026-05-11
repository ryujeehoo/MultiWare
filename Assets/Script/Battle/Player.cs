using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IEffectTarget
{
    public static Player Instance;
    public float maxHealth = 100f;
    public float currentHealth;
    public int gold = 0;
    private int defense = 0;
    public float healMultiplier = 1.0f;
    public System.Action<float, float> OnHealthChanged;
    public System.Action<int> OnGoldChanged;
    public List<Relic> relics = new List<Relic>(); // 유물 리스트
    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴 안 됨
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }
    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnGoldChanged?.Invoke(gold);
        // 전투 시작 유물 발동
        foreach (var relic in relics)
        {
            relic.OnBattleStart(this);
        }
    }

    public void TakeDamage(float damage)
    {
        if (BattleManager.Instance != null &&
            BattleManager.Instance.IsInvincible())
        {
            Debug.Log("무적! 피해 0");
            return;
        }

        if (BattleManager.Instance != null &&
            BattleManager.Instance.isWeakened)
        {
            damage *= 0.8f;
            Debug.Log($"약화 효과! 받는 피해 -20% → {damage}");
        }

        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(damage) - ConsumeDefense());
        currentHealth -= finalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        foreach (var relic in relics)
            relic.OnTakeDamage(this, finalDamage);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ApplyEffect(Effect effect)
    {
        effect.Apply(this);
    }
    public void ApplyEffect(StatusEffect effect)
    {
        // 같은 종류 효과가 이미 있으면 중첩
        StatusEffect existing = activeEffects.Find(e => e.effectName == effect.effectName);
        if (existing != null && existing is PoisonEffect existingPoison && effect is PoisonEffect newPoison)
        {
            existingPoison.stack += newPoison.stack; // 중첩 추가
            Debug.Log($"독 중첩! 현재 중첩: {existingPoison.stack}");
        }
        else
        {
            activeEffects.Add(effect);
            Debug.Log($"플레이어에게 {effect.effectName} 적용! 중첩: {((PoisonEffect)effect).stack}");
        }
    }

    public void TickEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].OnTurnEnd(this);
            if (activeEffects[i].IsExpired())
            {
                Debug.Log($"{activeEffects[i].effectName} 효과 종료!");
                activeEffects.RemoveAt(i);
            }
        }
    }
    public void OnTurnStart()
    {
        foreach (var relic in relics)
        {
            relic.OnTurnStart(this);
        }
    }
    public void AddHealMultiplier(float amount)
    {
        healMultiplier += amount;
        Debug.Log($"회복 배율 추가! 현재 배율: {healMultiplier}");
    }
    public void Heal(float amount)
    {
        float finalHeal = amount * healMultiplier;
        currentHealth += Mathf.RoundToInt(finalHeal);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"플레이어 HP 회복! +{Mathf.RoundToInt(finalHeal)} 현재 HP: {currentHealth}");
    }
    public void AddDefense(int amount)
    {
        defense += amount;
        Debug.Log($"방어막 +{amount} (현재: {defense})");
    }

    public int ConsumeDefense()
    {
        int current = defense;
        defense = 0;
        return current;
    }
}