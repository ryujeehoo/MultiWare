using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public GameState currentState;
    public CardFan cardFan;
    public int turnCount = 0;
    public int bonusDrawCount = 0;
    [Header("UI")]
    public CanvasGroup inputBlocker;
    public Button endTurnButton;

    // player 필드 제거 → Player.Instance로 통일
    private Player Player => Player.Instance;

    [Header("Enemy")]
    public MonsterSpawner spawner;
    private EnemyController[] enemies;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshEnemies();
        StartPlayerTurn();
    }

    public void RefreshEnemies()
    {
        if (spawner == null)
        {
            enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            Debug.Log($"MonsterSpawner 없음 → Scene에서 {enemies.Length}개 찾음");
            return;
        }

        List<EnemyController> list = new List<EnemyController>();
        foreach (GameObject obj in spawner.spawnedMonsters)
        {
            if (obj == null) continue;
            EnemyController ec = obj.GetComponent<EnemyController>();
            if (ec != null) list.Add(ec);
        }
        enemies = list.ToArray();
    }

    public void OnMonsterDied()
    {
        Debug.Log("OnMonsterDied 호출됨!");

        Monster[] aliveMonsters = FindObjectsByType<Monster>(FindObjectsSortMode.None);
        Debug.Log($"남은 몬스터 수: {aliveMonsters.Length}");

        if (aliveMonsters.Length == 0)
        {
            Debug.Log("모든 몬스터 사망!");
            foreach (var relic in Player.relics)
                relic.OnBattleEnd(Player);
            GameManager.Instance.OnVictory();
        }
    }

    public void OnClickEndTurn()
    {
        if (currentState != GameState.PlayerTurn)
            return;

        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        currentState = GameState.EnemyTurn;
        SetInputBlock(true);
        endTurnButton.interactable = false;
        Debug.Log("적 턴 시작");

        Player.LogActiveEffects();
        // ↑ Player.Instance로 올바른 플레이어 참조
        Player.TickEffects();

        Monster[] monsters = FindObjectsByType<Monster>(FindObjectsSortMode.None);
        foreach (Monster monster in monsters)
            monster.ResetDefense();

        if (BattleManager.Instance != null && BattleManager.Instance.isSlowed)
        {
            Debug.Log("둔화 효과! 적 행동 1턴 지연");
            BattleManager.Instance.isSlowed = false;
        }
        else
        {
            RefreshEnemies();
            foreach (EnemyController enemy in enemies)
            {
                if (enemy != null)
                    yield return StartCoroutine(enemy.EnemyAction());
            }
        }

        monsters = FindObjectsByType<Monster>(FindObjectsSortMode.None);
        foreach (Monster monster in monsters)
            monster.TickEffects();

        if (BattleManager.Instance != null)
            BattleManager.Instance.OnTurnEnd();

        Debug.Log("적 턴 종료");
        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        turnCount++;
        currentState = GameState.PlayerTurn;
        Player.OnTurnStart();
        // ↑ Player.Instance로 올바른 플레이어 참조
        SetInputBlock(false);
        endTurnButton.interactable = true;

        int currentCount = cardFan.GetCurrentCardCount();
        int drawCount = 8 - currentCount + bonusDrawCount;
        bonusDrawCount = 0;

        drawCount -= Player.GetBindReduction();
        // ↑ Player.Instance로 올바른 플레이어 참조 → 속박 정상 작동!
        drawCount = Mathf.Max(0, drawCount);

        Debug.Log($"현재 카드: {currentCount}장 / 드로우할 카드: {drawCount}장");

        if (drawCount > 0)
        {
            if (CardDealAnimator.Instance != null)
                StartCoroutine(CardDealAnimator.Instance.DealCardsAnimation(drawCount));
            else
                cardFan.DrawStartingHand();
        }

        foreach (var relic in Player.relics)
            relic.OnTurnStart(Player);
        RefreshAllMonsterIntents();
        Debug.Log("플레이어 턴 시작");
    }

    void SetInputBlock(bool block)
    {
        inputBlocker.blocksRaycasts = block;
        inputBlocker.interactable = !block;
    }

    void RefreshAllMonsterIntents()
    {
        foreach (Monster m in SpawnManager.Instance.GetAliveMonsters())
        {
            if (m == null || m.gameObject == null) continue; // ← 추가
            EnemyController ec = m.GetComponent<EnemyController>();
            if (ec != null) ec.RefreshIntentUI();
        }
    }
}