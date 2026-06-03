using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Game/Monster Data")]
public class MonsterData : ScriptableObject
{
    public string monsterName;
    public int maxHP;
    public int gold;
    public List<MonsterAction> actionPattern;

    [Header("HP Ʈ���� ����")]
    public List<HPTriggerAction> hpTriggers;

    [Header("공격 이펙트")]
    public float chargeDuration = 0.2f;
    public float chargeDistance = 1.5f;
    public float shakeStrength = 0.2f;
    public Sprite hitImage;                // 피격 시 화면에 표시될 몬스터 이미지
}

[System.Serializable]
public class HPTriggerAction
{
    [Range(0, 1)]
    public float hpThreshold;
    public MonsterAction action;
}
