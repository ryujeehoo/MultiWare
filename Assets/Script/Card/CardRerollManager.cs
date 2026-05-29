using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardRerollManager : MonoBehaviour
{
    public static CardRerollManager Instance;

    [Header("리롤 설정")]
    public int rerollCost = 5;

    [Header("UI 연결")]
    public Button rerollButton;
    public TMP_Text rerollButtonText;

    // 리롤 모드 상태
    public bool isRerollMode = false;

    // 리롤 선택된 카드들
    private List<CardView> selectedForReroll = new List<CardView>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateButtonText();
    }

    // 리롤 버튼 클릭
    public void OnClickRerollButton()
    {
        if (!isRerollMode)
        {
            isRerollMode = true;

            // 기존 전투 선택 카드 초기화!
            if (CardSelectManager.Instance != null)
                CardSelectManager.Instance.ClearSelected();

            // 기존에 선택된 카드들 위치 초기화
            CardView[] allCards = FindObjectsByType<CardView>(FindObjectsSortMode.None);
            foreach (CardView cv in allCards)
            {
                if (cv.isSelected)
                    cv.ToggleSelect();
            }

            Debug.Log("리롤 모드 ON! 리롤할 카드를 선택하세요.");
            UpdateButtonText();
        }
        else
        {
            if (selectedForReroll.Count == 0)
            {
                Debug.Log("리롤할 카드를 선택하세요!");
                isRerollMode = false;
                UpdateButtonText();
                return;
            }

            int totalCost = rerollCost * selectedForReroll.Count;

            if (GoldManager.Instance.currentGold < totalCost)
            {
                Debug.Log($"골드 부족! 필요: {totalCost}G");
                return;
            }

            GoldManager.Instance.SpendGold(totalCost);
            Debug.Log($"리롤! -{totalCost}G / {selectedForReroll.Count}장 교체");

            // 리롤 소리 재생! ← 여기 추가
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCardDraw();

            int rerollCount = selectedForReroll.Count;
            List<CardView> cardsToRemove = new List<CardView>(selectedForReroll);
            selectedForReroll.Clear();

            // CardSelectManager도 초기화! ← 추가
            if (CardSelectManager.Instance != null)
                CardSelectManager.Instance.ClearSelected();

            foreach (CardView cv in cardsToRemove)
            {
                if (cv != null && cv.gameObject != null)
                {
                    CardFan fan = FindFirstObjectByType<CardFan>();
                    if (fan != null)
                        fan.RemoveCardObject(cv.gameObject);
                    Destroy(cv.gameObject);
                }
            }

            if (CardDealAnimator.Instance != null && TurnManager.Instance != null)
                TurnManager.Instance.StartCoroutine(
                    CardDealAnimator.Instance.DealCardsAnimation(rerollCount));

            isRerollMode = false;
            UpdateButtonText();
        }
    }

    // 리롤 카드 선택/해제
    public void ToggleRerollSelect(CardView card)
    {
        if (!isRerollMode) return;

        if (selectedForReroll.Contains(card))
        {
            selectedForReroll.Remove(card);
            Debug.Log($"리롤 해제: {card.cardData.cardName}");
        }
        else
        {
            selectedForReroll.Add(card);
            Debug.Log($"리롤 선택: {card.cardData.cardName} / 비용: {rerollCost * selectedForReroll.Count}G");
        }

        UpdateButtonText();
    }

    // 버튼 텍스트 업데이트
    // UpdateButtonText() 수정
    private void UpdateButtonText()
    {
        if (rerollButtonText == null) return;

        if (!isRerollMode)
        {
            rerollButtonText.text = "Reroll";
        }
        else
        {
            int totalCost = rerollCost * selectedForReroll.Count;
            if (selectedForReroll.Count > 0)
                rerollButtonText.text = $"Confirm\n({selectedForReroll.Count} -{totalCost}G)";
            else
                rerollButtonText.text = "Select\nCards";
        }
    }
}