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
    public bool hasAttackedWhileShocked = false; // ���� �� ù ���� ����
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
            SceneManager.sceneLoaded += OnSceneLoaded; // �� �߰�
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // �� �߰�
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
        if (BattleManager.Instance != null &&
            BattleManager.Instance.IsInvincible())
        {
            Debug.Log("무적! 피해 0");
            return;
        }

        int dmg = Mathf.RoundToInt(damage);

        // 방어막 먼저 적용
        if (defense > 0)
        {
            int absorbed = Mathf.Min(defense, dmg);
            defense -= absorbed;
            dmg -= absorbed;
            OnDefenseChanged?.Invoke(defense);
            Debug.Log($"방어막 흡수! 남은 방어막: {defense} / 남은 데미지: {dmg}");
        }

        // 남은 데미지 HP에 적용
        if (dmg > 0)
        {
            currentHealth -= dmg;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        foreach (var relic in relics)
            relic.OnTakeDamage(this, dmg);

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
            existingPoison.stack += newPoison.stack; // ���� �� �ջ�
            Debug.Log($"�� ��ø! ���� ��: {existingPoison.stack}");
        }
        else if (existing != null && existing is ShockEffect existingShock && effect is ShockEffect newShock)
        {
            existingShock.stack += newShock.stack;
            Debug.Log($"���� ��ø! ���� ��ø: {existingShock.stack}");
        }
        else if (existing != null && existing is BindEffect existingBind && effect is BindEffect newBind)
        {
            existingBind.stack += newBind.stack;
            Debug.Log($"�ӹ� ��ø! ���� ��ø: {existingBind.stack}");
        }
        else
        {
            activeEffects.Add(effect);
            Debug.Log($"�÷��̾�� {effect.effectName} ����!");
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
                Debug.Log($"{activeEffects[i].effectName} ȿ�� ����!");
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
        Debug.Log($"ȸ�� ���� �߰�! ���� ����: {healMultiplier}");
    }

    public void Heal(float amount)
    {
        float finalHeal = amount * healMultiplier;
        currentHealth += Mathf.RoundToInt(finalHeal);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"�÷��̾� HP ȸ��! +{Mathf.RoundToInt(finalHeal)} ���� HP: {currentHealth}");
    }

    public void AddDefense(int amount)
    {
        defense += amount;
        OnDefenseChanged?.Invoke(defense);
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
            Debug.Log("[����] �� ��° ����! ����ī��� ���� ���");
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
            Debug.Log("[�����̻�] ����");
            return;
        }

        foreach (var effect in activeEffects)
        {
            if (effect is PoisonEffect poison)
                Debug.Log($"[�����̻�] �� - ���� ��: {poison.stack}");
            else if (effect is ShockEffect shock)
                Debug.Log($"[�����̻�] ���� - ���� ��: {shock.stack}");
            else if (effect is BindEffect bind)
                Debug.Log($"[�����̻�] �ӹ� - ���� ��: {bind.stack}");
        }
    }

   
}