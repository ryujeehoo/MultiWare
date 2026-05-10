using UnityEngine;

public enum ActionType { Attack, Defend, Poison, MultiAttack, Summon }

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

    public float GetValue(int turnCount)
    {
        if (scaleWithTurn)
            return value + (scaleAmount * turnCount);
        return value;
    }
}