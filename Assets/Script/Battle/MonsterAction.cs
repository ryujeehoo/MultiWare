using UnityEngine;

public enum ActionType { Attack, Defend, Poison }

[System.Serializable]
public class MonsterAction
{
    public ActionType actionType;
    public float value; // Poison이면 중첩 수

    [Header("턴 비례 강화")]
    public bool scaleWithTurn = false;
    public float scaleAmount = 0f;

    public float GetValue(int turnCount)
    {
        if (scaleWithTurn)
            return value + (scaleAmount * turnCount);
        return value;
    }
}