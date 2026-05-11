using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }
    public List<Monster> GetAliveMonsters() => aliveMonsters;
    [Header("컴포넌트 연결")]
    public MonsterSpawner spawner;

    private List<Monster> aliveMonsters = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        spawner.SpawnFromEncounterTable();
        aliveMonsters.Clear();

        foreach (var obj in spawner.spawnedMonsters)
        {
            if (obj == null) continue;
            Monster m = obj.GetComponent<Monster>();
            if (m != null) aliveMonsters.Add(m);
        }
    }

    public void SummonMonster(GameObject prefab)
    {
        GameObject spawned = spawner.SpawnSingleMonster(prefab);
        if (spawned == null)
        {
            Debug.LogWarning("소환 슬롯이 가득 찼습니다!");
            return;
        }

        Monster m = spawned.GetComponent<Monster>();
        if (m != null)
        {
            aliveMonsters.Add(m);
            Debug.Log($"{prefab.name} 소환 완료! 현재 몬스터 수: {aliveMonsters.Count}");
        }
    }

    public void OnMonsterDied(Monster monster)
    {
        aliveMonsters.Remove(monster);
        if (aliveMonsters.Count == 0)
            GameManager.Instance.OnVictory();
    }

    public void OnPlayerDead()
    {
        GameManager.Instance.OnDefeat();
    }
}