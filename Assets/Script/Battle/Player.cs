using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IEffectTarget
{
    public static Player Instance;
    public float maxHealth = 100f;
    public float currentHealth;
    public int gold = 0;
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
        // 무적 상태면 피해 0 (턴 내내 유지)
        if (BattleManager.Instance != null &&
            BattleManager.Instance.IsInvincible())
        {
            Debug.Log("무적! 피해 0");
            return;
        }

        // 약화 효과: 적 공격력 -20%
        if (BattleManager.Instance != null &&
            BattleManager.Instance.isWeakened)
        {
            damage *= 0.8f;
            Debug.Log($"약화 효과! 받는 피해 -20% → {damage}");
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        foreach (var relic in relics)
            relic.OnTakeDamage(this, damage);

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
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"플레이어 HP 회복! 현재 HP: {currentHealth}");
    }
    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"골드 추가 +{amount} (현재 골드: {gold})");
        OnGoldChanged?.Invoke(gold);
    }

    public bool SpendGold(int amount)
    {
        if (gold < amount)
        {
            Debug.Log("골드 부족!");
            return false;
        }

        gold -= amount;
        OnGoldChanged?.Invoke(gold);
        return true;
    }

}