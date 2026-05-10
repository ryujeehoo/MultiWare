using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public Player player;
    private MonsterData data;
    private int patternIndex = 0;
    private int turnCount = 0;

    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        if (player == null)
            Debug.LogError($"[{gameObject.name}] Player를 찾을 수 없습니다!");

        Monster monster = GetComponent<Monster>();
        if (monster != null && monster.Data != null)
            data = monster.Data;
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

        MonsterAction action = data.actionPattern[patternIndex];

        switch (action.actionType)
        {
            case ActionType.Attack:
                yield return StartCoroutine(DoAttack(action.GetValue(turnCount)));
                break;
            case ActionType.Defend:
                yield return StartCoroutine(DoDefend(action.GetValue(turnCount)));
                break;
            case ActionType.Poison:
                yield return StartCoroutine(DoPoison(action.GetValue(turnCount)));
                break;
            case ActionType.MultiAttack:
                yield return StartCoroutine(DoMultiAttack(action.GetValue(turnCount), action.attackCount));
                break;
            case ActionType.Summon:
                yield return StartCoroutine(DoSummon(action.summonPrefab, (int)action.GetValue(turnCount)));
                break;
        }

        patternIndex = (patternIndex + 1) % data.actionPattern.Count;
        turnCount++;
    }

    IEnumerator DoAttack(float damage)
    {
        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
        {
            damage *= 0.8f;
            Debug.Log($"약화! 공격력 -20% → {damage}");
        }

        Debug.Log($"[{gameObject.name}] 공격! {damage} 데미지");
        yield return new WaitForSeconds(1f);
        new DamageEffect(damage).Apply(player);
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator DoDefend(float amount)
    {
        Debug.Log($"[{gameObject.name}] 방어! {amount} 방어력");
        yield return new WaitForSeconds(1f);
        GetComponent<Monster>().AddDefense((int)amount);
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator DoPoison(float stack)
    {
        Debug.Log($"[{gameObject.name}] 독 공격! {stack} 중첩");
        yield return new WaitForSeconds(1f);
        player.ApplyEffect(new PoisonEffect((int)stack));
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator DoMultiAttack(float damage, int count)
    {
        Debug.Log($"[{gameObject.name}] 멀티 어택! {damage} 데미지 x {count}회");
        for (int i = 0; i < count; i++)
        {
            yield return new WaitForSeconds(0.5f);
            new DamageEffect(damage).Apply(player);
            Debug.Log($"[{gameObject.name}] {i + 1}번째 공격! {damage} 데미지");
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator DoSummon(GameObject prefab, int count)
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
}