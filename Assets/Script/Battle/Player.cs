using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, IEffectTarget
{
    public static Player Instance;
    public float maxHealth = 100f;
    public float currentHealth;
    public int gold = 0;
    private int defense = 0;
    public float healMultiplier = 1.0f;
    public float heartHealMultiplier = 1.0f;
    public float spadeBonus = 1.0f;
    public bool hasAttackedWhileShocked = false; // 감전 중 첫 공격 여부
    public System.Action<float, float> OnHealthChanged;
    public System.Action<int> OnGoldChanged;
    public List<Relic> relics = new List<Relic>();
    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentHealth = maxHealth;
            SceneManager.sceneLoaded += OnSceneLoaded; // ← 추가
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // ← 추가
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnGoldChanged?.Invoke(gold);
    }
    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnGoldChanged?.Invoke(gold);
        foreach (var relic in relics)
            relic.OnBattleStart(this);
    }

    public void TakeDamage(float damage)
    {
        if (BattleManager.Instance != null && BattleManager.Instance.IsInvincible())
        {
            Debug.Log("무적! 피해 0");
            return;
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
        PlayerEffectDisplay.Instance?.Refresh();
    }

    public void ApplyEffect(StatusEffect effect)
    {
        StatusEffect existing = activeEffects.Find(e => e.effectName == effect.effectName);

        if (existing != null && existing is PoisonEffect existingPoison && effect is PoisonEffect newPoison)
        {
            existingPoison.stack += newPoison.stack; // 지속 턴 합산
            Debug.Log($"독 중첩! 남은 턴: {existingPoison.stack}");
        }
        else if (existing != null && existing is ShockEffect existingShock && effect is ShockEffect newShock)
        {
            existingShock.stack += newShock.stack;
            Debug.Log($"감전 중첩! 현재 중첩: {existingShock.stack}");
        }
        else if (existing != null && existing is BindEffect existingBind && effect is BindEffect newBind)
        {
            existingBind.stack += newBind.stack;
            Debug.Log($"속박 중첩! 현재 중첩: {existingBind.stack}");
        }
        else
        {
            activeEffects.Add(effect);
            Debug.Log($"플레이어에게 {effect.effectName} 적용!");
        }
    }


    public int GetBindReduction()
    {
        int reduction = 0;
        foreach (var effect in activeEffects)
        {
            if (effect is BindEffect bind)
                reduction += bind.drawReduction;
        }
        return reduction;
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
        PlayerEffectDisplay.Instance?.Refresh();
    }

    public void OnTurnStart()
    {
        hasAttackedWhileShocked = false;
        foreach (var relic in relics)
            relic.OnTurnStart(this);
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
    public bool IsShocked()
    {
        foreach (var effect in activeEffects)
        {
            if (effect is ShockEffect)
                return true;
        }
        return false;
    }
    public bool ShouldForceHighCard()
    {
        if (!IsShocked()) return false;

        if (hasAttackedWhileShocked)
        {
            Debug.Log("[감전] 두 번째 공격! 하이카드로 강제 계산");
            return true;
        }

        hasAttackedWhileShocked = true;
        return false;
    }
    public bool WillForceHighCard()
    {
        return IsShocked() && hasAttackedWhileShocked;
    }
    public void LogActiveEffects()
    {
        if (activeEffects.Count == 0)
        {
            Debug.Log("[상태이상] 없음");
            return;
        }

        foreach (var effect in activeEffects)
        {
            if (effect is PoisonEffect poison)
                Debug.Log($"[상태이상] 독 - 남은 턴: {poison.stack}");
            else if (effect is ShockEffect shock)
                Debug.Log($"[상태이상] 감전 - 남은 턴: {shock.stack}");
            else if (effect is BindEffect bind)
                Debug.Log($"[상태이상] 속박 - 남은 턴: {bind.stack}");
        }
    }
}