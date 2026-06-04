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
    public float heartDefenseMultiplier = 1.0f;
    public float spadeBonus = 1.0f;
    public bool hasAttackedWhileShocked = false; // ê°ì „ ì¤?ì²?ê³µê²© ?¬ë?
    public System.Action<float, float> OnHealthChanged;
    public System.Action<int> OnGoldChanged;
    public List<Relic> relics = new List<Relic>();
    public List<StatusEffect> activeEffects = new List<StatusEffect>();
    public System.Action<int> OnDefenseChanged;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentHealth = maxHealth;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
    }

    public void TakeDamage(float damage)
    {
        if (BattleManager.Instance != null &&
            BattleManager.Instance.IsInvincible())
        {
            Debug.Log("ë¬´ì ! ?¼í•´ 0");
            return;
        }

        int dmg = Mathf.RoundToInt(damage);

        if (defense > 0)
        {
            int absorbed = Mathf.Min(defense, dmg);
            defense -= absorbed;
            dmg -= absorbed;
            OnDefenseChanged?.Invoke(defense);
        }
        int finalDamage = dmg;

        if (finalDamage > 0)
        {
            currentHealth -= finalDamage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        if (finalDamage > 0)
            HitEffectUI.ShowDamageVignetteGlobal();

        foreach (var relic in relics)
            relic.OnTakeDamage(this, finalDamage);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
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
            existingPoison.stack += newPoison.stack; // ??ì¤‘ì²© ?©ì‚°
            Debug.Log($"??ì¤‘ì²©! ?„ì¬ ?¤íƒ: {existingPoison.stack}");
        }
        else if (existing != null && existing is ShockEffect existingShock && effect is ShockEffect newShock)
        {
            existingShock.stack += newShock.stack;
            Debug.Log($"ê°ì „ ì¤‘ì²©! ?„ì¬ ì¤‘ì²©: {existingShock.stack}");
        }
        else if (existing != null && existing is BindEffect existingBind && effect is BindEffect newBind)
        {
            existingBind.stack += newBind.stack;
            Debug.Log($"?ë°• ì¤‘ì²©! ?„ì¬ ì¤‘ì²©: {existingBind.stack}");
        }
        else
        {
            activeEffects.Add(effect);
            Debug.Log($"?Œë ˆ?´ì–´?ê²Œ {effect.effectName} ë¶€??");
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
                Debug.Log($"{activeEffects[i].effectName} ?¨ê³¼ ì¢…ë£Œ!");
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
        Debug.Log($"?Œë³µ ë°°ìœ¨ ì¶”ê?! ?„ì¬ ë°°ìœ¨: {healMultiplier}");
    }

    public void Heal(float amount)
    {
        float finalHeal = amount * healMultiplier;
        currentHealth += Mathf.RoundToInt(finalHeal);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"?Œë ˆ?´ì–´ HP ?Œë³µ! +{Mathf.RoundToInt(finalHeal)} ?„ì¬ HP: {currentHealth}");
    }

    public void AddDefense(int amount)
    {
        defense += amount;
OnDefenseChanged?.Invoke(defense);
        Debug.Log($"Defense +{amount} (current: {defense})");
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
            Debug.Log("[Shock] Second attack: force high card.");
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
            Debug.Log("[?íƒœ?´ìƒ] ?†ìŒ");
            return;
        }

        foreach (var effect in activeEffects)
        {
            if (effect is PoisonEffect poison)
                Debug.Log($"[?íƒœ?´ìƒ] ??- ?„ì¬ ?¤íƒ: {poison.stack}");
            else if (effect is ShockEffect shock)
                Debug.Log($"[?íƒœ?´ìƒ] ê°ì „ - ?„ì¬ ?¤íƒ: {shock.stack}");
            else if (effect is BindEffect bind)
                Debug.Log($"[?íƒœ?´ìƒ] ?ë°• - ?„ì¬ ?¤íƒ: {bind.stack}");
        }
    }
    void Die()
    {
        Debug.Log("?Œë ˆ?´ì–´ ?¬ë§!");
        // GameManager?????´ë™???´ë‹¹?˜ë‹ˆ ë¨¼ì? ?¸ì¶œ
        if (GameManager.Instance != null)
            GameManager.Instance.OnDefeat();
        ResetAll();
    }
    void ResetAll()
    {
        relics.Clear();
        activeEffects.Clear();
        currentHealth = maxHealth;
        gold = 0;
        defense = 0;
        healMultiplier = 1.0f;
        heartHealMultiplier = 1.0f;
        heartDefenseMultiplier = 1.0f;
        hasAttackedWhileShocked = false;

        if (GoldManager.Instance != null)
        {
            Destroy(GoldManager.Instance.gameObject);
            GoldManager.Instance = null;
        }

        if (DeckManager.Instance != null)
        {
            Destroy(DeckManager.Instance.gameObject);
            DeckManager.Instance = null;
        }

        GameManager.DestroyInstance();

        Instance = null;
        Destroy(gameObject);
    }
}