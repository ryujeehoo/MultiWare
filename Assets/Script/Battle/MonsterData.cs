using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Game/Monster Data")]
public class MonsterData : ScriptableObject
{
    public string monsterName;
    public int maxHP;
    public int gold;
    public List<MonsterAction> actionPattern;

    [Header("HP 트리거 패턴")]
    public List<HPTriggerAction> hpTriggers;

    [Header("아군 사망 트리거")]
    public List<AllyDeathTrigger> allyDeathTriggers;
}

[System.Serializable]
public class HPTriggerAction
{
    [Range(0, 1)]
    public float hpThreshold;
    public MonsterAction action;
}

[System.Serializable]
public class AllyDeathTrigger
{
    public string allyMonsterName;
    public int patternIndex;
    public bool boostPoison;
}