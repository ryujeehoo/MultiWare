using UnityEngine;

public enum ActionType { Attack, Defend, Poison, MultiAttack, Summon, AttackBuff, Heal, Bind, Shock, AllyDefend, AttackAndEffect, Reflect }

public enum AttackEffectType { None, Poison, Shock }

[System.Serializable]
public class MonsterAction
{
    public ActionType actionType;
    public float value;

    [Header("턴 비례 강화")]
    public bool scaleWithTurn = false;
    public float scaleAmount = 0f;

    [Header("멀티 어택 설정 (MultiAttack일 때만 사용)")]
    public int attackCount = 2;

    [Header("소환 설정 (Summon일 때만 사용)")]
    public GameObject summonPrefab;

    [Header("독 설정 (Poison일 때만 사용)")]
    public int poisonStack = 2;

    [Header("감전 설정 (Shock / AttackAndEffect-Shock일 때만 사용)")]
    public int shockDamagePerActivation = 5;
    public int shockDuration = 3;

    [Header("AttackAndEffect 설정")]
    public AttackEffectType attackEffectType = AttackEffectType.None;
    public int attackPoisonStack = 2;  // AttackAndEffect-Poison일 때만 사용

    [Header("반사 설정 (Reflect일 때만 사용)")]
    public int reflectDamage = 3;
    public int reflectDefense = 0;

    public float GetValue(int turnCount)
    {
        if (scaleWithTurn)
            return value + (scaleAmount * turnCount);
        return value;
    }
}