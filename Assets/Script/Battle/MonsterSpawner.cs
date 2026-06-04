using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("UI Settings")]
    public Transform spawnParent;

    [Header("고정 슬롯 위치")]
    public RectTransform[] slots = new RectTransform[3];

    [Header("편성 테이블")]
    public EncounterTable encounterTable;

    public GameObject[] spawnedMonsters { get; private set; } = new GameObject[3];

    public void SpawnFromEncounterTable()
    {
        if (encounterTable == null || encounterTable.encounters.Length == 0)
        {
            Debug.LogError("EncounterTable이 비어있습니다.");
            return;
        }

        int index = GameManager.Instance.GetRandomEncounterIndex(encounterTable.encounters.Length);
        MonsterEncounter encounter = encounterTable.encounters[index];
        Debug.Log($"선택된 편성: {encounter.encounterName}");
        BattleBackgroundManager backgroundManager = BattleBackgroundManager.GetOrCreate();
        if (backgroundManager != null)
            backgroundManager.ApplyEncounter(encounter);

        ClearSpawnedMonsters();

        for (int i = 0; i < 3; i++)
        {
            GameObject prefab = encounter.monsterPrefabSlots[i];
            if (prefab == null) continue;

            if (prefab.GetComponent<Monster>() == null)
            {
                Debug.LogError($"{prefab.name} 프리팹에 Monster 컴포넌트가 없습니다.");
                continue;
            }

            spawnedMonsters[i] = SpawnAtSlot(prefab, slots[i]);
        }
    }

    public GameObject SpawnSingleMonster(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("소환할 프리팹이 없습니다!");
            return null;
        }

        for (int i = 0; i < spawnedMonsters.Length; i++)
        {
            if (spawnedMonsters[i] == null)
            {
                GameObject obj = SpawnAtSlot(prefab, slots[i]);
                spawnedMonsters[i] = obj;
                Debug.Log($"슬롯 {i}에 {prefab.name} 소환!");
                return obj;
            }
        }

        return null;
    }

    private GameObject SpawnAtSlot(GameObject prefab, RectTransform slot)
    {
        GameObject obj = Instantiate(prefab, spawnParent);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = slot.anchorMin;
        rect.anchorMax = slot.anchorMax;
        rect.anchoredPosition = slot.anchoredPosition;
        rect.sizeDelta = slot.sizeDelta;
        return obj;
    }

    public void ClearSpawnedMonsters()
    {
        for (int i = 0; i < spawnedMonsters.Length; i++)
        {
            if (spawnedMonsters[i] != null)
                Destroy(spawnedMonsters[i]);
            spawnedMonsters[i] = null;
        }
    }
}