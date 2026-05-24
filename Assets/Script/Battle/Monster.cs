using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Monster : MonoBehaviour, IDropHandler, IEffectTarget
{
    public MonsterData Data;
    public StatusEffectDisplay effectDisplay;
    private int currentHP;
    private int maxHP; // 스케일링된 최대 HP 저장
    private int defense = 0;
    private Animator anim;
    private List<StatusEffect> activeEffects = new List<StatusEffect>();

    [Header("UI")]
    public Image hpBar;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI defenseText;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        float scale = GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        maxHP = Mathf.RoundToInt(Data.maxHP * scale);
        currentHP = maxHP;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (Data == null) return;

        float ratio = (float)currentHP / Data.maxHP;

        if (hpBar != null)
        {
            hpBar.fillAmount = ratio;
        }

        if (hpText != null)
        {
            hpText.fontSize = 15;
            hpText.text = $"{currentHP}/{maxHP}"; // Data.maxHP → maxHP로 변경

            if ((float)currentHP / maxHP <= 0.3f)
                hpText.color = new Color(1f, 0.5f, 0f);
            else
                hpText.color = new Color(0.9f, 0.2f, 0.2f);
        }

        if (defenseText != null)
        {
            defenseText.fontSize = 15;
            if (defense > 0)
            {
                defenseText.text = $"{defense}";
                defenseText.color = new Color(0.7f, 0.7f, 0.75f);
                defenseText.gameObject.SetActive(true);
            }
            else
            {
                defenseText.gameObject.SetActive(false);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        int dmg = Mathf.RoundToInt(damage);
        int finalDamage = Mathf.Max(0, dmg - defense);
        defense = Mathf.Max(0, defense - dmg); // defense = 0 → 남은 방어막 유지
        currentHP -= finalDamage;
        currentHP = Mathf.Max(0, currentHP);
        if (anim != null) anim.SetTrigger("Damage");
        Debug.Log($"몬스터 HP: {currentHP} (데미지: {finalDamage}, 남은 방어막: {defense})");
        UpdateUI();
        CheckReflect();
        EnemyController ec = GetComponent<EnemyController>();
        if (ec != null) ec.RefreshIntentUI();
    }

    public void ResetDefense()
    {
        defense = 0;
        Debug.Log($"[{gameObject.name}] 방어력 초기화!");
        UpdateUI();
    }

    public void AddDefense(int amount)
    {
        defense += amount;
        Debug.Log($"[{gameObject.name}] 방어력 {defense} 획득!");
        UpdateUI();
    }

    public void ApplyEffect(StatusEffect effect)
    {
        StatusEffect existing = activeEffects.Find(e => e.effectName == effect.effectName);
        if (existing != null && existing is PoisonEffect existingPoison && effect is PoisonEffect newPoison)
        {
            existingPoison.stack += newPoison.stack;
            Debug.Log($"{Data.monsterName} 독 중첩! 현재 중첩: {existingPoison.stack}");
        }
        else
        {
            activeEffects.Add(effect);
            Debug.Log($"{Data.monsterName}에게 {effect.effectName} 적용!");
        }
        effectDisplay?.Refresh(activeEffects);
    }

    void CheckReflect()
    {
        foreach (var effect in activeEffects)
        {
            if (effect is ReflectEffect reflect)
            {
                Player.Instance.TakeDamage(reflect.reflectDamage);
                Debug.Log($"[반사] 플레이어에게 {reflect.reflectDamage} 반사 데미지!");
            }
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
        effectDisplay?.Refresh(activeEffects);
    }

    public void OnDrop(PointerEventData eventData)
    {
        // CardBundle 방식 제거 - CardView 드래그 방식으로 변경됨
    }

    int CalculateFinalDamage(List<CardData> cards, HandResult result)
    {
        int baseDamage = result.baseDamage;

        float focusMultiplier = BattleManager.Instance?.ConsumeFocus() ?? 1.0f;
        float damageMultiplier = BattleManager.Instance?.ConsumeDamageMultiplier() ?? 1.0f;
        float handRankMultiplier = BattleManager.Instance?.ConsumeHandRankMultiplier() ?? 1.0f;

        baseDamage = Mathf.RoundToInt(baseDamage * focusMultiplier * damageMultiplier * handRankMultiplier);

        if (BattleManager.Instance != null && BattleManager.Instance.isExposed)
        {
            baseDamage = Mathf.RoundToInt(baseDamage * 1.2f);
            Debug.Log($"노출 효과! 데미지 +20% → {baseDamage}");
        }

        int spadeCount = 0;
        foreach (CardData c in cards)
            if (c.suit == SuitType.Spade) spadeCount++;

        float spadeRatio = (float)spadeCount / 5f;

        float bonusPercent = 0f;
        if (spadeRatio >= 1.0f) bonusPercent = 0.65f;
        else if (spadeRatio >= 0.8f) bonusPercent = 0.45f;
        else if (spadeRatio >= 0.6f) bonusPercent = 0.30f;
        else if (spadeRatio >= 0.4f) bonusPercent = 0.18f;
        else if (spadeRatio >= 0.2f) bonusPercent = 0.08f;

        int finalDamage = Mathf.RoundToInt(baseDamage * (1f + bonusPercent));

        if (bonusPercent > 0)
            Debug.Log($"스페이드 보너스 +{bonusPercent * 100}% / 최종 데미지: {finalDamage}");

        return finalDamage;
    }

    void ApplySuitEffects(List<CardData> cards)
    {
        int spadeCount = 0, heartCount = 0,
            diamondCount = 0, cloverCount = 0;

        foreach (CardData c in cards)
        {
            switch (c.suit)
            {
                case SuitType.Spade: spadeCount++; break;
                case SuitType.Heart: heartCount++; break;
                case SuitType.Diamond: diamondCount++; break;
                case SuitType.Clover: cloverCount++; break;
            }
        }

        float heartRatio = (float)heartCount / 5f;
        float diamondRatio = (float)diamondCount / 5f;
        float cloverRatio = (float)cloverCount / 5f;

        if (heartCount > 0)
        {
            float healAmount = 0f;

            if (heartRatio >= 1.0f) healAmount = 45f;
            else if (heartRatio >= 0.8f) healAmount = 30f;
            else if (heartRatio >= 0.6f) healAmount = 20f;
            else if (heartRatio >= 0.4f) healAmount = 12f;
            else if (heartRatio >= 0.2f) healAmount = 5f;

            if (healAmount > 0)
            {
                Player player = FindFirstObjectByType<Player>();
                if (player != null)
                    player.Heal(healAmount * player.heartHealMultiplier);
                Debug.Log($"하트 회복 +{healAmount * player.heartHealMultiplier} HP");
            }
        }

        if (diamondCount > 0)
        {
            int goldAmount = 0;

            if (diamondRatio >= 1.0f) goldAmount = 40;
            else if (diamondRatio >= 0.8f) goldAmount = 25;
            else if (diamondRatio >= 0.6f) goldAmount = 15;
            else if (diamondRatio >= 0.4f) goldAmount = 8;
            else if (diamondRatio >= 0.2f) goldAmount = 3;

            if (goldAmount > 0)
            {
                int finalGold = Mathf.RoundToInt(goldAmount * GoldManager.Instance.diamondGoldMultiplier);
                GoldManager.Instance.AddGold(finalGold);
                Debug.Log($"다이아 골드 +{finalGold}G");
            }
        }

        if (cloverCount > 0)
            ApplyCloverEffect(cloverRatio);
    }

    void ApplyCloverEffect(float ratio)
    {
        if (ratio <= 0) return;

        if (ratio < 0.4f)
        {
            Debug.Log("클로버 약화: 적 공격력 -20% (2턴) 자동 발동!");
            ApplyDebuff(ratio);
            return;
        }

        string debuffName = "", debuffDesc = "";
        string buffName = "", buffDesc = "";

        if (ratio < 0.6f)
        {
            debuffName = "Expose";
            debuffDesc = "Enemy DEF -20% (2 turns)";
            buffName = "Inspiration";
            buffDesc = "Draw +1 card now";
        }
        else if (ratio < 0.8f)
        {
            debuffName = "Panic";
            debuffDesc = "Enemy acts randomly (1 turn)";
            buffName = "Focus";
            buffDesc = "Next hand DMG +40%";
        }
        else if (ratio < 1.0f)
        {
            debuffName = "Slow";
            debuffDesc = "Enemy action delayed 1 turn";
            buffName = "Lucky";
            buffDesc = "Draw +2 cards now";
        }
        else
        {
            debuffName = "Curse";
            debuffDesc = "2 random debuffs applied";
            buffName = "Invincible";
            buffDesc = "Take 0 damage this turn";
        }

        if (CloverChoiceUI.Instance != null)
        {
            CloverChoiceUI.Instance.ShowChoice(
                debuffName, debuffDesc,
                buffName, buffDesc,
                () => ApplyDebuff(ratio),
                () => ApplyBuff(ratio)
            );
        }
    }

    void ApplyDebuff(float ratio)
    {
        if (BattleManager.Instance == null) return;

        if (ratio < 0.4f)
        {
            BattleManager.Instance.ApplyWeaken(2);
            Debug.Log("약화: 적 공격력 -20% (2턴)");
        }
        else if (ratio < 0.6f)
        {
            BattleManager.Instance.ApplyExpose(2);
            Debug.Log("노출: 적 방어력 -20% (2턴)");
        }
        else if (ratio < 0.8f)
        {
            BattleManager.Instance.ApplyPanic();
            Debug.Log("공황: 적이 이번 턴 랜덤 행동 (1턴)");
        }
        else if (ratio < 1.0f)
        {
            BattleManager.Instance.ApplySlow();
            Debug.Log("둔화: 적 행동 1턴 지연");
        }
        else
        {
            int[] debuffs = { 0, 1, 2, 3 };
            System.Random rng = new System.Random();
            int first = debuffs[rng.Next(debuffs.Length)];
            int second;
            do { second = debuffs[rng.Next(debuffs.Length)]; }
            while (second == first);

            ApplyRandomDebuff(first);
            ApplyRandomDebuff(second);
            Debug.Log("저주: 랜덤 디버프 2종 동시 발동!");
        }
    }

    void ApplyRandomDebuff(int index)
    {
        if (BattleManager.Instance == null) return;
        switch (index)
        {
            case 0: BattleManager.Instance.ApplyWeaken(2); break;
            case 1: BattleManager.Instance.ApplyExpose(2); break;
            case 2: BattleManager.Instance.ApplyPanic(); break;
            case 3: BattleManager.Instance.ApplySlow(); break;
        }
    }

    void ApplyBuff(float ratio)
    {
        if (ratio < 0.6f)
        {
            Debug.Log("영감: 즉시 드로우 +1장");
            if (TurnManager.Instance != null && CardDealAnimator.Instance != null)
                TurnManager.Instance.StartCoroutine(
                    CardDealAnimator.Instance.DealCardsAnimation(1));
        }
        else if (ratio < 0.8f)
        {
            Debug.Log("집중: 다음 족보 데미지 +40%");
            if (BattleManager.Instance != null)
                BattleManager.Instance.ApplyFocus();
        }
        else if (ratio < 1.0f)
        {
            Debug.Log("천운: 즉시 드로우 +2장");
            if (TurnManager.Instance != null && CardDealAnimator.Instance != null)
                TurnManager.Instance.StartCoroutine(
                    CardDealAnimator.Instance.DealCardsAnimation(2));
        }
        else
        {
            Debug.Log("무적: 이번 턴 받는 피해 0");
            if (BattleManager.Instance != null)
                BattleManager.Instance.ApplyInvincible();
        }
    }
    public float GetHPRatio()
    {
        return (float)currentHP / maxHP; // Data.maxHP → maxHP로 변경
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP); // Data.maxHP → maxHP로 변경
        Debug.Log($"[{gameObject.name}] HP {amount} 회복! 현재 HP: {currentHP}");
        UpdateUI();
    }
    public void SelfDestruct()
    {
        Debug.Log($"[{gameObject.name}] 자폭으로 사망!");
        GoldManager.Instance.AddGold(Data.gold);
        gameObject.SetActive(false);
        TurnManager.Instance.OnMonsterDied();
        Destroy(gameObject);
    }
    public void ReceiveCardBundle(HandResult result, List<CardData> cards)
    {
        foreach (var relic in Player.Instance.relics)
            relic.OnAttack(Player.Instance, cards, this, result);

        // 감전 체크 - 두 번째 공격부터 하이카드 강제
        if (Player.Instance.ShouldForceHighCard())
        {
            result = new HandResult(HandRankType.HighCard, HandResult.CalculateDamage(HandRankType.HighCard, cards), cards);
            Debug.Log($"[감전] 하이카드 강제! 데미지: {result.baseDamage}");
        }

        int finalDamage = CalculateFinalDamage(cards, result);
        TakeDamage(finalDamage);
        Debug.Log($"몬스터에게 {finalDamage} 데미지!");

        ApplySuitEffects(cards);

        if (currentHP <= 0)
            Die();
        PlayerEffectDisplay.Instance?.Refresh();
    }
    // 몬스터 사망 시 남은 몬스터를 계산할 때 사망 전에 검사하는 것을 수정함
    void Die()
    {
        //if (anim != null) anim.SetTrigger("Die");
        Debug.Log("Die() 호출됨!");
        GoldManager.Instance.AddGold(Data.gold);
        gameObject.SetActive(false);
        TurnManager.Instance.OnMonsterDied();
        Destroy(gameObject);
    }
}