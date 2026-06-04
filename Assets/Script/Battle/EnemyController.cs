using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    private Player Player => Player.Instance;

    private MonsterData data;
    private Animator anim;
    private MonsterAttackVFX attackVFX;
    private MonsterAction currentAction;
    private int patternIndex = 0;
    private int turnCount = 0;
    private float attackBonus = 0f;
    private int actionStack = 0;
    private HashSet<int> triggeredHPActions = new HashSet<int>();

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        attackVFX = GetComponent<MonsterAttackVFX>();
        if (attackVFX == null)
            attackVFX = gameObject.AddComponent<MonsterAttackVFX>();

        Monster monster = GetComponent<Monster>();
        if (monster != null && monster.Data != null)
        {
            data = monster.Data;
            RefreshIntentUI();
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] MonsterData is missing.");
        }
    }

    public IEnumerator EnemyAction()
    {
        if (data == null || data.actionPattern.Count == 0)
        {
            Debug.LogError($"[{gameObject.name}] Action pattern is missing.");
            yield break;
        }

        if (BattleManager.Instance != null && BattleManager.Instance.isPanicked)
        {
            int random = Random.Range(0, 2);
            if (random == 0)
            {
                Debug.Log("Panic: enemy skipped its action.");
                yield return new WaitForSeconds(1f);
                yield break;
            }

            Debug.Log("Panic: enemy still acted.");
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
        if (data.hpTriggers == null)
        {
            result(false);
            yield break;
        }

        Monster monster = GetComponent<Monster>();
        if (monster == null)
        {
            result(false);
            yield break;
        }

        for (int i = 0; i < data.hpTriggers.Count; i++)
        {
            if (triggeredHPActions.Contains(i)) continue;

            float hpRatio = monster.GetHPRatio();
            if (hpRatio <= data.hpTriggers[i].hpThreshold)
            {
                triggeredHPActions.Add(i);
                Debug.Log($"{data.monsterName} HP trigger activated.");
                yield return StartCoroutine(ExecuteAction(data.hpTriggers[i].action));
                result(true);
                yield break;
            }
        }

        result(false);
    }

    IEnumerator ExecuteAction(MonsterAction action)
    {
        GetComponent<Monster>()?.PlaySound(GetComponent<Monster>()?.attackSound);
        currentAction = action;

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
                yield return StartCoroutine(DoAttackAndEffect(action.GetValue(turnCount), action.attackEffectType, action.effectDuration));
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
            case ActionType.Lurk:
                yield return StartCoroutine(DoLurk());
                break;
        }

        currentAction = null;
    }

    MonsterAttackVFXType ResolveMotionType()
    {
        if (currentAction == null || currentAction.attackVFXType == MonsterAttackVFXType.Auto)
            return MonsterAttackVFXType.BiteStrike;

        switch (currentAction.attackVFXType)
        {
            case MonsterAttackVFXType.Dash:
                return MonsterAttackVFXType.Charge;
            case MonsterAttackVFXType.StoneCrush:
            case MonsterAttackVFXType.RootSlam:
                return MonsterAttackVFXType.HeavyAttack;
            default:
                return currentAction.attackVFXType;
        }
    }

    float GetChargeDuration(MonsterAttackVFXType motionType)
    {
        switch (motionType)
        {
            case MonsterAttackVFXType.Charge:
                return Mathf.Max(data.chargeDuration * 0.3f, 0.05f);
            case MonsterAttackVFXType.HeavyAttack:
                return Mathf.Max(data.chargeDuration * 0.85f, 0.16f);
            default:
                return Mathf.Max(data.chargeDuration * 0.6f, 0.01f);
        }
    }

    IEnumerator ChargeAndReturn()
    {
        if (data == null || Player == null) yield break;

        Vector3 origin = transform.position;
        Vector3 rawDir = Player.transform.position - transform.position;
        Vector3 dir = new Vector3(rawDir.x, rawDir.y * 0.25f, 0f).normalized;
        if (dir.sqrMagnitude <= 0.0001f)
            dir = Vector3.left;

        Vector3 target = origin + dir * data.chargeDistance;
        target.y = Mathf.Clamp(target.y, origin.y - 0.25f, origin.y + 0.25f);
        MonsterAttackVFXType motionType = ResolveMotionType();
        float t = 0f;

        if (motionType == MonsterAttackVFXType.BiteStrike)
        {
            Vector3 pullTarget = origin - dir * 0.28f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.07f;
                transform.position = Vector3.Lerp(origin, pullTarget, t);
                yield return null;
            }

            origin = pullTarget;
            t = 0f;
        }

        while (t < 1f)
        {
            t += Time.deltaTime / GetChargeDuration(motionType);
            transform.position = Vector3.Lerp(origin, target, t);
            yield return null;
        }

        HitEffectUI.Instance?.Show(data.hitImage);
        if (attackVFX != null && Player != null)
        {
            Vector3 effectTarget = Player.transform.position;
            effectTarget.y = Mathf.Clamp(effectTarget.y, transform.position.y - 0.35f, transform.position.y + 0.35f);
            yield return StartCoroutine(attackVFX.PlayAttack(currentAction, origin, effectTarget));
        }
        if (data.shakeStrength > 0f)
            CameraShake.Instance?.Shake(0.3f, data.shakeStrength);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.14f;
            transform.position = Vector3.Lerp(target, origin, t);
            yield return null;
        }

        transform.position = origin;
    }

    IEnumerator PlayCurrentAnticipationVFX()
    {
        if (attackVFX != null)
            yield return StartCoroutine(attackVFX.PlayAnticipation(currentAction));
    }

    IEnumerator PlayCurrentSupportVFX()
    {
        if (attackVFX != null)
            yield return StartCoroutine(attackVFX.PlaySupport(currentAction));
    }

    IEnumerator PlayCurrentTargetVFX()
    {
        if (attackVFX != null && Player != null)
            yield return StartCoroutine(attackVFX.PlayAttack(currentAction, transform.position, Player.transform.position));
    }

    public IEnumerator DoAttack(float damage)
    {
        damage *= GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
            damage *= 0.8f;

        Debug.Log($"[{gameObject.name}] Attack {damage}");
        if (anim != null) anim.SetTrigger("Attack");
        yield return StartCoroutine(PlayCurrentAnticipationVFX());
        yield return new WaitForSeconds(0.12f);
        yield return StartCoroutine(ChargeAndReturn());
        new DamageEffect(damage).Apply(Player);
        yield return new WaitForSeconds(0.12f);
    }

    public IEnumerator DoDefend(float amount)
    {
        yield return StartCoroutine(PlayCurrentSupportVFX());
        Debug.Log($"[{gameObject.name}] Defend {amount}");
        yield return new WaitForSeconds(1f);
        GetComponent<Monster>().AddDefense((int)amount);
        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator DoPoison(int duration)
    {
        yield return StartCoroutine(PlayCurrentTargetVFX());
        Debug.Log($"[{gameObject.name}] Poison {duration}");
        yield return new WaitForSeconds(1f);
Player.ApplyEffect(new PoisonEffect(duration));
        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator DoMultiAttack(float damage, int count)
    {
        damage *= GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
            damage *= 0.8f;

        Debug.Log($"[{gameObject.name}] MultiAttack {damage} x {count}");
        for (int i = 0; i < count; i++)
        {
            if (anim != null) anim.SetTrigger("Attack");
            yield return StartCoroutine(PlayCurrentAnticipationVFX());
            yield return new WaitForSeconds(0.06f);
            yield return StartCoroutine(ChargeAndReturn());
            new DamageEffect(damage).Apply(Player);
Debug.Log($"[{gameObject.name}] Multi hit {i + 1}: {damage}");
            yield return new WaitForSeconds(0.08f);
        }
    }

    public IEnumerator DoSummon(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
        {
            Debug.LogWarning($"[{gameObject.name}] Summon skipped.");
            yield break;
        }

        yield return StartCoroutine(PlayCurrentSupportVFX());
        Debug.Log($"[{gameObject.name}] Summon {prefab.name} x {count}");
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < count; i++)
        {
            SpawnManager.Instance.SummonMonster(prefab);
            yield return new WaitForSeconds(0.12f);
        }
    }

    public IEnumerator DoBuffAttack(float amount)
    {
        yield return StartCoroutine(PlayCurrentSupportVFX());
        attackBonus += amount;
        Debug.Log($"[{gameObject.name}] AttackBuff +{amount}. Bonus: {attackBonus}");
        yield return new WaitForSeconds(1f);
    }

    public IEnumerator DoHeal(float amount)
    {
        yield return StartCoroutine(PlayCurrentSupportVFX());
        Monster monster = GetComponent<Monster>();
        if (monster == null) yield break;

        Debug.Log($"[{gameObject.name}] Heal {amount}");
        yield return new WaitForSeconds(1f);
        monster.Heal((int)amount);
        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator DoBind(int duration)
    {
        yield return StartCoroutine(PlayCurrentTargetVFX());
        Debug.Log($"[{gameObject.name}] Bind {duration}");
        yield return new WaitForSeconds(1f);
Player.ApplyEffect(new BindEffect(duration));
        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator DoShock(int duration)
    {
        yield return StartCoroutine(PlayCurrentTargetVFX());
        Debug.Log($"[{gameObject.name}] Shock {duration}");
        yield return new WaitForSeconds(1f);
Player.ApplyEffect(new ShockEffect(duration));
        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator DoAllyDefend(float amount)
    {
        List<Monster> allies = SpawnManager.Instance.GetAliveMonsters()
            .FindAll(m => m != null && m.gameObject != null && m.gameObject != gameObject);

        if (allies.Count == 0)
        {
            Debug.Log($"[{gameObject.name}] AllyDefend skipped.");
            yield break;
        }

        yield return StartCoroutine(PlayCurrentSupportVFX());
        Debug.Log($"[{gameObject.name}] AllyDefend {amount}");
        yield return new WaitForSeconds(1f);

        foreach (Monster monster in allies)
        {
            monster.AddDefense((int)amount);
            Debug.Log($"{monster.Data.monsterName} gains defense {amount}");
        }

        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator DoAttackAndEffect(float damage, AttackEffectType effectType, int duration)
    {
        damage *= GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
            damage *= 0.8f;

        Debug.Log($"[{gameObject.name}] AttackAndEffect {effectType} {damage}");
        if (anim != null) anim.SetTrigger("Attack");
        yield return StartCoroutine(PlayCurrentAnticipationVFX());
        yield return new WaitForSeconds(0.12f);
        yield return StartCoroutine(ChargeAndReturn());
        new DamageEffect(damage).Apply(Player);

        switch (effectType)
        {
            case AttackEffectType.Poison:
                Player.ApplyEffect(new PoisonEffect(duration));
                break;
            case AttackEffectType.Shock:
                Player.ApplyEffect(new ShockEffect(duration));
                break;
        }

        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator DoReflect(int reflectDamage, int reflectDefense)
    {
        yield return StartCoroutine(PlayCurrentSupportVFX());
        Monster monster = GetComponent<Monster>();
        if (monster == null) yield break;

        Debug.Log($"[{gameObject.name}] Reflect {reflectDamage}, defense {reflectDefense}");
        yield return new WaitForSeconds(1f);
        monster.ApplyEffect(new ReflectEffect(reflectDamage));
        if (reflectDefense > 0)
            monster.AddDefense(reflectDefense);
        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator DoStackUp(int amount)
    {
        yield return StartCoroutine(PlayCurrentSupportVFX());
        actionStack += amount;
        GetComponent<Monster>()?.effectDisplay?.SetStack(actionStack);
        Debug.Log($"[{gameObject.name}] Stack +{amount}. Current: {actionStack}");
        yield return new WaitForSeconds(1f);
    }

    IEnumerator DoStackAttack(float damagePerStack)
    {
        damagePerStack *= GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
            damagePerStack *= 0.8f;

        int totalDamage = (int)damagePerStack * actionStack;
        Debug.Log($"[{gameObject.name}] StackAttack {damagePerStack} x {actionStack} = {totalDamage}");
        if (anim != null) anim.SetTrigger("Attack");
        yield return StartCoroutine(PlayCurrentAnticipationVFX());
        yield return new WaitForSeconds(0.12f);
        yield return StartCoroutine(ChargeAndReturn());
        new DamageEffect(totalDamage).Apply(Player);
        // actionStack = 0;  ← 삭제
        // GetComponent<Monster>()?.effectDisplay?.SetStack(actionStack);  ← 삭제
        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator DoSelfDestruct(float damage)
    {
        damage *= GameManager.Instance != null ? GameManager.Instance.GetStageScaling() : 1f;
        if (BattleManager.Instance != null && BattleManager.Instance.isWeakened)
            damage *= 0.8f;

        Debug.Log($"[{gameObject.name}] SelfDestruct {damage}");
        if (anim != null) anim.SetTrigger("Attack");
        yield return StartCoroutine(PlayCurrentAnticipationVFX());
        yield return new WaitForSeconds(0.12f);
        yield return StartCoroutine(ChargeAndReturn());
        new DamageEffect(damage).Apply(Player);
        yield return new WaitForSeconds(0.12f);

        Monster monster = GetComponent<Monster>();
        if (monster != null)
            monster.SelfDestruct();
    }

    IEnumerator DoLurk()
    {
        Monster monster = GetComponent<Monster>();
        if (monster == null) yield break;

        Debug.Log($"[{gameObject.name}] Lurk.");
        monster.isLurking = true;
        monster.effectDisplay?.SetLurk(true);
        yield return new WaitForSeconds(1f);
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
            ui.UpdateIntent(GetNextAction(), turnCount, attackBonus, actionStack);
    }
}