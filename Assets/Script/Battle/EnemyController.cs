using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    // player �ʵ� ���� �� Player.Instance ������Ƽ�� ����
    private Player Player => Player.Instance;

    private MonsterData data;
    private Animator anim;
    private int patternIndex = 0;
    private int turnCount = 0;
    private float attackBonus = 0f;
    private int actionStack = 0;
    private HashSet<int> triggeredHPActions = new HashSet<int>();

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();

        Monster monster = GetComponent<Monster>();
        if (monster != null && monster.Data != null)
        {
            data = monster.Data;
            RefreshIntentUI();
        }
        else
            Debug.LogError($"[{gameObject.name}] MonsterData�� �����ϴ�!");
    }

    public IEnumerator EnemyAction()
    {
        if (data == null || data.actionPattern.Count == 0)
        {
            Debug.LogError($"[{gameObject.name}] ������ �����ϴ�!");
            yield break;
        }

        if (BattleManager.Instance != null && BattleManager.Instance.isPanicked)
        {
            int random = Random.Range(0, 2);
            if (random == 0)
            {
                Debug.Log("��Ȳ! ���� ȥ�������� �������� ���ߴ�!");
                yield return new WaitForSeconds(1f);
                yield break;
            }
            else
            {
                Debug.Log("��Ȳ! ���� ȥ���������� �����ߴ�!");
            }
        }

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
                Debug.Log($"{data.monsterName} HP Ʈ���� �ߵ�! ���� ������ ���� ������");
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
        damage *= scale;

        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
        {
            damage *= 0.8f;
            Debug.Log($"��ȭ! ���ݷ� -20% �� {damage}");
        }

        Debug.Log($"[{gameObject.name}] ����! {damage} ������");
        if (anim != null) anim.SetTrigger("Attack");
        yield return new WaitForSeconds(1f);
        new DamageEffect(damage).Apply(Player); // �� Player.Instance
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoDefend(float amount)
    {
        Debug.Log($"[{gameObject.name}] ���! {amount} ����");
        yield return new WaitForSeconds(1f);
        GetComponent<Monster>().AddDefense((int)amount);
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoPoison(int duration)
    {
        Debug.Log($"[{gameObject.name}] �� ����! {duration}��");
        yield return new WaitForSeconds(1f);
        Player.ApplyEffect(new PoisonEffect(duration)); // �� Player.Instance
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoMultiAttack(float damage, int count)
    {
        float scale = GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        damage *= scale;

        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
        {
            damage *= 0.8f;
            Debug.Log($"��ȭ! ���ݷ� -20% �� {damage}");
        }

        Debug.Log($"[{gameObject.name}] ��Ƽ ����! {damage} ������ x {count}ȸ");
        for (int i = 0; i < count; i++)
        {
            if (anim != null) anim.SetTrigger("Attack");
            yield return new WaitForSeconds(0.5f);
            new DamageEffect(damage).Apply(Player);
            Debug.Log($"[{gameObject.name}] {i + 1}��° ����! {damage} ������");
            yield return new WaitForSeconds(0.3f);
        }
    }

    public IEnumerator DoSummon(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
        {
            Debug.LogWarning($"[{gameObject.name}] ��ȯ ���� ������");
            yield break;
        }

        Debug.Log($"[{gameObject.name}] {prefab.name} {count}���� ��ȯ!");
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
        Debug.Log($"[{gameObject.name}] ���ݷ� +{amount} ����! ���� ���ʽ�: {attackBonus}");
        yield return new WaitForSeconds(1f);
    }

    public IEnumerator DoHeal(float amount)
    {
        Monster monster = GetComponent<Monster>();
        if (monster == null) yield break;

        Debug.Log($"[{gameObject.name}] HP {amount} ȸ��!");
        yield return new WaitForSeconds(1f);
        monster.Heal((int)amount);
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoBind(int duration)
    {
        Debug.Log($"[{gameObject.name}] �ӹ�! ��ο� -1 ({duration}��)");
        yield return new WaitForSeconds(1f);
        Player.ApplyEffect(new BindEffect(duration)); // �� Player.Instance
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoShock(int duration)
    {
        Debug.Log($"[{gameObject.name}] ����! {duration}��");
        yield return new WaitForSeconds(1f);
        Player.ApplyEffect(new ShockEffect(duration)); // �� Player.Instance
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoAllyDefend(float amount)
    {
        // �ڽ� ���� ����ִ� �Ʊ� Ȯ��
        List<Monster> allies = SpawnManager.Instance.GetAliveMonsters()
            .FindAll(m => m != null && m.gameObject != null && m.gameObject != gameObject);

        // �Ʊ��� ������ �׳� ��ŵ
        if (allies.Count == 0)
        {
            Debug.Log($"[{gameObject.name}] �Ʊ��� ���� �� �ο� ��ŵ!");
            yield break;
        }

        Debug.Log($"[{gameObject.name}] �Ʊ� ��ü �� {amount} �ο�!");
        yield return new WaitForSeconds(1f);

        foreach (Monster monster in allies)
        {
            monster.AddDefense((int)amount);
            Debug.Log($"{monster.Data.monsterName}���� �� {amount} �ο�!");
        }

        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoAttackAndEffect(float damage, AttackEffectType effectType, int duration)
    {
        float scale = GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        damage *= scale;

        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
            damage *= 0.8f;

        Debug.Log($"[{gameObject.name}] ���� + {effectType}! {damage} ������");
        yield return new WaitForSeconds(1f);
        new DamageEffect(damage).Apply(Player); // �� Player.Instance

        switch (effectType)
        {
            case AttackEffectType.Poison:
                Player.ApplyEffect(new PoisonEffect(duration)); // �� Player.Instance
                Debug.Log($"�� {duration}�� �ο�!");
                break;
            case AttackEffectType.Shock:
                Player.ApplyEffect(new ShockEffect(duration)); // �� Player.Instance
                Debug.Log($"���� {duration}�� �ο�!");
                break;
        }

        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator DoReflect(int reflectDamage, int reflectDefense)
    {
        Monster monster = GetComponent<Monster>();
        if (monster == null) yield break;

        Debug.Log($"[{gameObject.name}] �ݻ� ���! �ݻ� {reflectDamage} / �� {reflectDefense}");
        yield return new WaitForSeconds(1f);
        monster.ApplyEffect(new ReflectEffect(reflectDamage));
        if (reflectDefense > 0)
            monster.AddDefense(reflectDefense);
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator DoStackUp(int amount)
    {
        actionStack += amount;
        Debug.Log($"[{gameObject.name}] ���� +{amount}! ���� ����: {actionStack}");
        yield return new WaitForSeconds(1f);
    }

    IEnumerator DoStackAttack(float damagePerStack)
    {
        float scale = GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        damagePerStack *= scale;

        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
        {
            damagePerStack *= 0.8f;
            Debug.Log($"��ȭ! ���ݷ� -20% �� {damagePerStack}");
        }

        int totalDamage = (int)damagePerStack * actionStack;
        Debug.Log($"[{gameObject.name}] ���� ����! {damagePerStack} �� {actionStack} = {totalDamage} ������");
        yield return new WaitForSeconds(1f);
        new DamageEffect(totalDamage).Apply(Player);
        actionStack = 0;
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator DoSelfDestruct(float damage)
    {
        float scale = GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        damage *= scale;

        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
        {
            damage *= 0.8f;
            Debug.Log($"��ȭ! ���ݷ� -20% �� {damage}");
        }

        Debug.Log($"[{gameObject.name}] ����! {damage} ������");
        yield return new WaitForSeconds(1f);
        new DamageEffect(damage).Apply(Player);
        yield return new WaitForSeconds(0.5f);

        Monster monster = GetComponent<Monster>();
        if (monster != null)
            monster.SelfDestruct();
    }

    public MonsterAction GetNextAction()
    {
        if (data == null || data.actionPattern.Count == 0) return null;

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