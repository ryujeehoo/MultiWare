using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckViewUI : MonoBehaviour
{
    public static DeckViewUI Instance;

    [Header("UI 연결")]
    public GameObject deckViewPanel;      // 팝업 패널
    public GameObject closeButton;  // ← 추가
    public Transform spadeRow;            // 스페이드 줄
    public Transform heartRow;            // 하트 줄
    public Transform cloverRow;           // 클로버 줄
    public Transform diamondRow;          // 다이아 줄
    public GameObject cardIconPrefab;     // 카드 아이콘 프리팹

    [Header("색상 설정")]
    public Color normalColor = Color.white;           // 남아있는 카드
    public Color usedColor = new Color(0.4f, 0.4f, 0.4f, 0.8f); // 사용된 카드 (회색)

    private void Awake()
    {
        Instance = this;
        if (deckViewPanel != null)
            deckViewPanel.SetActive(false);

        // CloseButton도 처음에 비활성화
        if (closeButton != null)
            closeButton.SetActive(false);
    }

    // View Deck 버튼 클릭
    public void OnClickViewDeck()
    {
        if (deckViewPanel == null) return;

        bool isActive = deckViewPanel.activeSelf;
        bool willOpen = !isActive;

        deckViewPanel.SetActive(willOpen);

        if (closeButton != null)
            closeButton.SetActive(willOpen);

        HideMonsters(willOpen);

        // Turn End 버튼 비활성화/활성화
        if (TurnManager.Instance != null)
            TurnManager.Instance.endTurnButton.interactable = !willOpen;

        if (willOpen)
            RefreshDeckView();
    }

    private void HideMonsters(bool hide)
    {
        // 방법 1: MonsterSpawner로 찾기
        MonsterSpawner spawner = FindFirstObjectByType<MonsterSpawner>();
        if (spawner != null)
        {
            spawner.gameObject.SetActive(!hide);
            return;
        }

        // 방법 2: SpawnManager로 찾기
        SpawnManager spawnManager = FindFirstObjectByType<SpawnManager>();
        if (spawnManager != null)
        {
            spawnManager.gameObject.SetActive(!hide);
            return;
        }

        // 방법 3: 직접 Monster 찾기
        Monster[] monsters = FindObjectsByType<Monster>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        foreach (Monster monster in monsters)
        {
            if (monster != null)
                monster.gameObject.SetActive(!hide);
        }
    }

    // 덱 현황 업데이트
    public void RefreshDeckView()
    {
        if (DeckManager.Instance == null) return;

        // 각 줄 초기화
        ClearRow(spadeRow);
        ClearRow(heartRow);
        ClearRow(cloverRow);
        ClearRow(diamondRow);

        // 카드 순서: A, 2~10, J, Q, K
        int[] numberOrder = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };

        foreach (int number in numberOrder)
        {
            AddCardIcon(SuitType.Spade, number, spadeRow);
            AddCardIcon(SuitType.Heart, number, heartRow);
            AddCardIcon(SuitType.Clover, number, cloverRow);
            AddCardIcon(SuitType.Diamond, number, diamondRow);
        }
    }

    // 카드 아이콘 추가
    private void AddCardIcon(SuitType suit, int number, Transform row)
    {
        if (row == null || cardIconPrefab == null) return;

        // allCards에서 해당 카드 찾기
        CardData card = DeckManager.Instance.allCards.Find(
            c => c.suit == suit &&
            (c.cardNumber == number || (number == 1 && c.cardNumber == 14)));

        if (card == null) return;

        // 아이콘 생성
        GameObject icon = Instantiate(cardIconPrefab, row);
        Image iconImage = icon.GetComponent<Image>();

        if (iconImage != null)
        {
            // 카드 이미지 적용
            if (card.cardSprite != null)
                iconImage.sprite = card.cardSprite;

            // 사용된 카드면 회색으로
            if (DeckManager.Instance.IsCardUsed(card))
                iconImage.color = usedColor;
            else
                iconImage.color = normalColor;
        }
    }

    // 줄 초기화
    private void ClearRow(Transform row)
    {
        if (row == null) return;
        foreach (Transform child in row)
            Destroy(child.gameObject);
    }
}