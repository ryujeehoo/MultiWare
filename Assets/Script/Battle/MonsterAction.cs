using UnityEngine;

public enum ActionType
{
    Attack, Defend, Poison, MultiAttack, Summon, AttackBuff,
    Heal, Bind, Shock, AllyDefend, AttackAndEffect, Reflect,
    StackUp, StackAttack, SelfDestruct, Lurk // ← 추가
}
public enum AttackEffectType { None, Poison, Shock }

public enum MonsterAttackVFXType
{
    Auto = 0,

    BiteStrike = 12,
    HeavyAttack = 13,
    Charge = 14,
    Poison = 15,
    RootBind = 16,
    Shield = 17,
    Summon = 18,
    Heal = 19,
    CrystalAttack = 20,
    Burrow = 21,

    // Legacy values kept stable for existing serialized assets.
    SlimeSplash = 1,
    RootSlam = 2,
    ElectricBolt = 3,
    PoisonSting = 4,
    Dash = 5,
    StoneCrush = 6,
    BurrowStrike = 7,
    ShieldPulse = 8,
    SummonPulse = 9,
    HealPulse = 10,
    SelfDestruct = 11
}

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

    [Header("효과 지속 턴 (Poison / Shock / AttackAndEffect일 때만 사용)")]
    public int effectDuration = 2;

    [Header("AttackAndEffect 설정")]
    public AttackEffectType attackEffectType = AttackEffectType.None;

    [Header("공격 연출")]
    public MonsterAttackVFXType attackVFXType = MonsterAttackVFXType.Auto;
    public Color attackVFXColor = Color.white;

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