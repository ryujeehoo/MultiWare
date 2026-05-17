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
    public Player player;

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
        //Monster Spawner가 없어서 뜨는 오류 방지용 코드
        if (spawner == null)
        {
            // spawner 없으면 Scene에서 직접 찾기
            enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            Debug.Log($"MonsterSpawner 없음 → Scene에서 {enemies.Length}개 찾음");
            return;
        }
        //여기까지가 오류 방지용 코드

        List<EnemyController> list = new List<EnemyController>();
        foreach (GameObject obj in spawner.spawnedMonsters)
        {
            if (obj == null) continue;
            EnemyController ec = obj.GetComponent<EnemyController>();
            if (ec != null) list.Add(ec);
        }
        enemies = list.ToArray();
    }
    /*
     RefreshEnemies()의 오류 방지용으로 변경 전 코드 
 public void RefreshEnemies()
 {
     List<EnemyController> list = new List<EnemyController>();
     foreach (GameObject obj in spawner.spawnedMonsters)
     {
         if (obj == null) continue;
         EnemyController ec = obj.GetComponent<EnemyController>();
         if (ec != null) list.Add(ec);
     }
     enemies = list.ToArray();
 }

*/
    //모든 몬스터 사망시 씬 전환
    public void OnMonsterDied()
    {
        Debug.Log("OnMonsterDied 호출됨!");

        Monster[] aliveMonsters = FindObjectsByType<Monster>(FindObjectsSortMode.None);
        Debug.Log($"남은 몬스터 수: {aliveMonsters.Length}");

        if (aliveMonsters.Length == 0)
        {
            Debug.Log("모든 몬스터 사망!");
            // 전투 종료 유물 발동
            foreach (var relic in Player.Instance.relics)
                relic.OnBattleEnd(Player.Instance);
            GameManager.Instance.OnVictory(); // ← 이걸로 변경
        }
    }

    // 턴 종료 버튼
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

        // 적 턴 시작 시 플레이어 상태이상 처리
        player.TickEffects();

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

        // 몬스터 상태이상 처리
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
        player.OnTurnStart();
        SetInputBlock(false);
        endTurnButton.interactable = true;

        int currentCount = cardFan.GetCurrentCardCount();
        int drawCount = 8 - currentCount + bonusDrawCount; // ← 7 → 8로 변경
        bonusDrawCount = 0;

        drawCount -= player.GetBindReduction();
        drawCount = Mathf.Max(0, drawCount);

        Debug.Log($"현재 카드: {currentCount}장 / 드로우할 카드: {drawCount}장");

        if (drawCount > 0)
        {
            if (CardDealAnimator.Instance != null)
                StartCoroutine(CardDealAnimator.Instance.DealCardsAnimation(drawCount));
            else
                cardFan.DrawStartingHand();
        }

        foreach (var relic in Player.Instance.relics)
            relic.OnTurnStart(Player.Instance);

        Debug.Log("플레이어 턴 시작");
    }

    void SetInputBlock(bool block)
    {
        inputBlocker.blocksRaycasts = block;
        inputBlocker.interactable = !block;
    }
}