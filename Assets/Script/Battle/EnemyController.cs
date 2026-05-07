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

        // 공황 상태 체크
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
        }

        patternIndex = (patternIndex + 1) % data.actionPattern.Count;
        turnCount++;
    }

    IEnumerator DoAttack(float damage)
    {
        // 약화 효과 적용
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
}