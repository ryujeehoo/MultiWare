using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    public Player player;
    private MonsterData data;
    private Animator anim;  // ← 추가
    private int patternIndex = 0;
    private int turnCount = 0;
    private float attackBonus = 0f;
    private int actionStack = 0;
    private HashSet<int> triggeredHPActions = new HashSet<int>();

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();  // ← 추가
        player = FindAnyObjectByType<Player>();
        if (player == null)
            Debug.LogError($"[{gameObject.name}] Player를 찾을 수 없습니다!");

        Monster monster = GetComponent<Monster>();
        if (monster != null && monster.Data != null)
        {
            data = monster.Data;
            RefreshIntentUI();
        }
        else
            Debug.LogError($"[{gameObject.name}] MonsterData가 없습니다!");
    }

    public IEnumerator EnemyAction()
    {
        if (data == null || data.actionPattern.Count == 0)
        {
            Debug.LogError($"[{gameObject.name}] 패턴이 없습니다!");
            yield break;
        }

        if (BattleManager.Instance != null && BattleManager.Instance.isPanicked)
        {
            int random = Random.Range(0, 2);
            if (random == 0)
            {
                Debug.Log("공황! 적이 혼란스러워 공격하지 못했다!");
                yield return new WaitForSeconds(1f);
                yield break;
            }
            else
            {
                Debug.Log("공황! 적이 혼란스럽지만 공격했다!");
            }
        }

        // HP 트리거 체크
        bool triggered = false;
        yield return StartCoroutine(CheckAndExecuteHPTriggers(result => triggered = result));

        if (!triggered)
        {
            MonsterAction action = data.actionPattern[patternIndex];
            yield return StartCoroutine(ExecuteAction(action));
            patternIndex = (patternIndex + 1) % data.actionPattern.Count;
        }

        turnCount++;
    }

    IEnumerator CheckAndExecuteHPTriggers(System.Action<bool> result)
    {
        if (data.hpTriggers == null) { result(false); yield break; }

        Monster monster = GetComponent<Monster>();
        if (monster == null) { result(false); yield break; }

        for (int i = 0; i < data.hpTriggers.Count; i++)
        {
            if (triggeredHPActions.Contains(i)) continue;

            float hpRatio = monster.GetHPRatio();
            if (hpRatio <= data.hpTriggers[i].hpThreshold)
            {
                triggeredHPActions.Add(i);
                Debug.Log($"{data.monsterName} HP 트리거 발동! 원래 패턴은 다음 턴으로");
                yield return StartCoroutine(ExecuteAction(data.hpTriggers[i].action));
                result(true);
                yield break;
            }
        }

        result(false);
    }

    IEnumerator ExecuteAction(MonsterAction action)
    {
        switch (action.actionType)
        {
            case ActionType.Attack:
                yield return StartCoroutine(DoAttack(action.GetValue(turnCount) + attackBonus));
                break;
            case ActionType.Defend:
                yield return StartCoroutine(DoDefend(action.GetValue(turnCount)));
                break;
            case ActionType.Poison:
                yield return StartCoroutine(DoPoison(action.effectDuration));
                break;
            case ActionType.MultiAttack:
                yield return StartCoroutine(DoMultiAttack(action.GetValue(turnCount) + attackBonus, action.attackCount));
                break;
            case ActionType.Summon:
                yield return StartCoroutine(DoSummon(action.summonPrefab, (int)action.GetValue(turnCount)));
                break;
            case ActionType.AttackBuff:
                yield return StartCoroutine(DoBuffAttack(action.GetValue(turnCount)));
                break;
            case ActionType.Heal:
                yield return StartCoroutine(DoHeal(action.GetValue(turnCount)));
                break;
            case ActionType.Bind:
                yield return StartCoroutine(DoBind((int)action.GetValue(turnCount)));
                break;
            case ActionType.Shock:
                yield return StartCoroutine(DoShock(action.effectDuration));
                break;
            case ActionType.AllyDefend:
                yield return StartCoroutine(DoAllyDefend(action.GetValue(turnCount)));
                break;
            case ActionType.AttackAndEffect:
                yield return StartCoroutine(DoAttackAndEffect(
                    action.GetValue(turnCount),
                    action.attackEffectType,
                    action.effectDuration));
                break;
            case ActionType.Reflect:
                yield return StartCoroutine(DoReflect(action.reflectDamage, action.reflectDefense));
                break;
            case ActionType.StackUp:
                yield return StartCoroutine(DoStackUp((int)action.GetValue(turnCount)));
                break;
            case ActionType.StackAttack:
                yield return StartCoroutine(DoStackAttack(action.GetValue(turnCount)));
                break;
            case ActionType.SelfDestruct:
                yield return StartCoroutine(DoSelfDestruct(action.GetValue(turnCount)));
                break;
        }
    }

    public IEnumerator DoAttack(float damage)
    {
        float scale = GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        damage = damage * scale;

        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
        {
            damage *= 0.8f;
            Debug.Log($"약화! 공격력 -20% → {damage}");
        }

        Debug.Log($"[{gameObject.name}] 공격! {damage} 데미지");
        if (anim != null) anim.SetTrigger("Attack");  // ← 추가
        yield return new WaitForSeconds(1f);
        new DamageEffect(damage).Apply(player);
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoDefend(float amount)
    {
        Debug.Log($"[{gameObject.name}] 방어! {amount} 방어력");
        yield return new WaitForSeconds(1f);
        GetComponent<Monster>().AddDefense((int)amount);
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoPoison(int duration)
    {
        Debug.Log($"[{gameObject.name}] 독 공격! {duration}턴");
        yield return new WaitForSeconds(1f);
        player.ApplyEffect(new PoisonEffect(duration));
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoMultiAttack(float damage, int count)
    {
        float scale = GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        damage = damage * scale;

        Debug.Log($"[{gameObject.name}] 멀티 어택! {damage} 데미지 x {count}회");
        for (int i = 0; i < count; i++)
        {
            if (anim != null) anim.SetTrigger("Attack");  // ← 추가
            yield return new WaitForSeconds(0.5f);
            new DamageEffect(damage).Apply(player);
            Debug.Log($"[{gameObject.name}] {i + 1}번째 공격! {damage} 데미지");
            yield return new WaitForSeconds(0.3f);
        }
    }

    public IEnumerator DoSummon(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
        {
            Debug.LogWarning($"[{gameObject.name}] 소환 조건 미충족");
            yield break;
        }

        Debug.Log($"[{gameObject.name}] {prefab.name} {count}마리 소환!");
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < count; i++)
        {
            SpawnManager.Instance.SummonMonster(prefab);
            yield return new WaitForSeconds(0.3f);
        }
    }

    public IEnumerator DoBuffAttack(float amount)
    {
        attackBonus += amount;
        Debug.Log($"[{gameObject.name}] 공격력 +{amount} 증가! 현재 보너스: {attackBonus}");
        yield return new WaitForSeconds(1f);
    }

    public IEnumerator DoHeal(float amount)
    {
        Monster monster = GetComponent<Monster>();
        if (monster == null) yield break;

        Debug.Log($"[{gameObject.name}] HP {amount} 회복!");
        yield return new WaitForSeconds(1f);
        monster.Heal((int)amount);
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoBind(int duration)
    {
        Debug.Log($"[{gameObject.name}] 속박! 드로우 -1 ({duration}턴)");
        yield return new WaitForSeconds(1f);
        player.ApplyEffect(new BindEffect(duration));
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoShock(int duration)
    {
        Debug.Log($"[{gameObject.name}] 감전! {duration}턴");
        yield return new WaitForSeconds(1f);
        player.ApplyEffect(new ShockEffect(duration));
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoAllyDefend(float amount)
    {
        Debug.Log($"[{gameObject.name}] 아군 전체 방어막 {amount} 부여!");
        yield return new WaitForSeconds(1f);

        foreach (Monster monster in SpawnManager.Instance.GetAliveMonsters())
        {
            if (monster.gameObject == gameObject) continue;
            monster.AddDefense((int)amount);
            Debug.Log($"{monster.Data.monsterName}에게 방어막 {amount} 부여!");
        }

        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoAttackAndEffect(float damage, AttackEffectType effectType, int duration)
    {
        float scale = GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        damage = damage * scale;

        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
            damage *= 0.8f;

        Debug.Log($"[{gameObject.name}] 공격 + {effectType}! {damage} 데미지");
        yield return new WaitForSeconds(1f);
        new DamageEffect(damage).Apply(player);

        switch (effectType)
        {
            case AttackEffectType.Poison:
                player.ApplyEffect(new PoisonEffect(duration));
                Debug.Log($"독 {duration}턴 부여!");
                break;
            case AttackEffectType.Shock:
                player.ApplyEffect(new ShockEffect(duration));
                Debug.Log($"감전 {duration}턴 부여!");
                break;
        }

        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoReflect(int reflectDamage, int reflectDefense)
    {
        Monster monster = GetComponent<Monster>();
        if (monster == null) yield break;

        Debug.Log($"[{gameObject.name}] 반사 방어! 반사 {reflectDamage} / 방어막 {reflectDefense}");
        yield return new WaitForSeconds(1f);
        monster.ApplyEffect(new ReflectEffect(reflectDamage)); // duration 제거
        if (reflectDefense > 0)
            monster.AddDefense(reflectDefense);
        yield return new WaitForSeconds(0.5f);
    }
    IEnumerator DoStackUp(int amount)
    {
        actionStack += amount;
        Debug.Log($"[{gameObject.name}] 스택 +{amount}! 현재 스택: {actionStack}");
        yield return new WaitForSeconds(1f);
    }

    IEnumerator DoStackAttack(float damagePerStack)
    {
        float scale = GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        damagePerStack = damagePerStack * scale;

        int totalDamage = (int)damagePerStack * actionStack;
        Debug.Log($"[{gameObject.name}] 스택 공격! {damagePerStack} × {actionStack} = {totalDamage} 데미지");
        yield return new WaitForSeconds(1f);
        new DamageEffect(totalDamage).Apply(player);
        actionStack = 0;
        yield return new WaitForSeconds(0.5f);
    }
    IEnumerator DoSelfDestruct(float damage)
    {
        float scale = GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        damage = damage * scale;

        Debug.Log($"[{gameObject.name}] 자폭! {damage} 데미지");
        yield return new WaitForSeconds(1f);
        new DamageEffect(damage).Apply(player);
        yield return new WaitForSeconds(0.5f);

        Monster monster = GetComponent<Monster>();
        if (monster != null)
            monster.SelfDestruct();
    }
    public MonsterAction GetNextAction()
    {
        if (data == null || data.actionPattern.Count == 0) return null;

        // HP 트리거 확인
        if (data.hpTriggers != null)
        {
            Monster monster = GetComponent<Monster>();
            if (monster != null)
            {
                for (int i = 0; i < data.hpTriggers.Count; i++)
                {
                    if (triggeredHPActions.Contains(i)) continue;
                    if (monster.GetHPRatio() <= data.hpTriggers[i].hpThreshold)
                        return data.hpTriggers[i].action;
                }
            }
        }

        return data.actionPattern[patternIndex];
    }

    public void RefreshIntentUI()
    {
        MonsterIntentUI ui = GetComponentInChildren<MonsterIntentUI>();
        if (ui != null)
            ui.UpdateIntent(GetNextAction(), turnCount, attackBonus);
    }
}