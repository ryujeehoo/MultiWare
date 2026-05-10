using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public GameState currentState;
    public CardFan cardFan;

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
        Monster[] monsters = FindObjectsByType<Monster>(FindObjectsSortMode.None);
        foreach (Monster monster in monsters)
            monster.ResetDefense();
        // 둔화 효과 먼저 체크 (OnTurnEnd 전에!)
        if (BattleManager.Instance != null && BattleManager.Instance.isSlowed)
        {
            Debug.Log("둔화 효과! 적 행동 1턴 지연");
            BattleManager.Instance.isSlowed = false;
        }
        else
        {
            foreach (EnemyController enemy in enemies)
            {
                if (enemy != null)
                    yield return StartCoroutine(enemy.EnemyAction());
            }
        }
        player.TickEffects(); // 추가 - 독 데미지 처리
        // 적 행동 끝난 후 턴 종료 처리
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnTurnEnd();

        Debug.Log("적 턴 종료");
        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        currentState = GameState.PlayerTurn;
        player.OnTurnStart();
        SetInputBlock(false);
        endTurnButton.interactable = true;

        // 7장 기준으로 변경
        int currentCount = cardFan.GetCurrentCardCount();
        int drawCount = 7 - currentCount;  // ← 5 → 7로 변경

        Debug.Log($"현재 카드: {currentCount}장 / 드로우할 카드: {drawCount}장");

        if (drawCount > 0)
        {
            if (CardDealAnimator.Instance != null)
                StartCoroutine(CardDealAnimator.Instance.DealCardsAnimation(drawCount));
            else
                cardFan.DrawStartingHand();
        }

        Debug.Log("플레이어 턴 시작");
    }

    void SetInputBlock(bool block)
    {
        inputBlocker.blocksRaycasts = block;
        inputBlocker.interactable = !block;
    }
}