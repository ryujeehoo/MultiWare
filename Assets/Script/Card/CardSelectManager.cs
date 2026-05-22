using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSelectManager : MonoBehaviour
{
    public static CardSelectManager Instance;

    [Header("선택 설정")]
    public int maxSelectCount = 5;

    [Header("UI 연결")]
    public TMP_Text handResultText;
    public Canvas mainCanvas;

    private List<CardView> selectedCards = new List<CardView>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void SelectCard(CardView card)
    {
        if (selectedCards.Count >= maxSelectCount)
        {
            Debug.Log("최대 5장까지 선택 가능합니다!");
            return;
        }

        selectedCards.Add(card);
        //감전 적용 체크용
        Debug.Log($"선택: {card.cardData.cardName} ({selectedCards.Count}장) / 감전: {Player.Instance.IsShocked()} / hasAttacked: {Player.Instance.hasAttackedWhileShocked}");
        UpdateUI();
    }

    public void DeselectCard(CardView card)
    {
        selectedCards.Remove(card);
        Debug.Log($"선택 해제: {card.cardData.cardName} ({selectedCards.Count}장)");
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (selectedCards.Count == 0)
        {
            if (handResultText != null)
                handResultText.text = "Select Cards";
            return;
        }

        List<CardData> cardDataList = new List<CardData>();
        foreach (CardView cv in selectedCards)
            cardDataList.Add(cv.cardData);

        HandResult result = HandEvaluator.Evaluate(cardDataList);

        // 노출 배율
        float exposeMultiplier = (BattleManager.Instance != null && BattleManager.Instance.isExposed) ? 1.2f : 1f;

        // 감전 상태면 두 번째 공격부터 하이카드 표시
        if (Player.Instance != null && Player.Instance.IsShocked() && Player.Instance.hasAttackedWhileShocked)
        {
            HandResult highCard = new HandResult(HandRankType.HighCard, HandResult.CalculateDamage(HandRankType.HighCard, cardDataList), cardDataList);
            int displayDmg = Mathf.RoundToInt(highCard.baseDamage * exposeMultiplier);
            if (handResultText != null)
                handResultText.text = $"{result.GetRankName()} → High Card (DMG: {displayDmg})";
        }
        else
        {
            int displayDmg = Mathf.RoundToInt(result.baseDamage * exposeMultiplier);
            if (handResultText != null)
                handResultText.text = $"{result.GetRankName()} (DMG: {displayDmg})";
        }
    }

    public void ShowMessage(string message)
    {
        if (handResultText != null)
            handResultText.text = message;
    }

    public List<CardView> GetSelectedCards()
    {
        return selectedCards;
    }

    // 선택 카드 초기화
    public void ClearSelected()
    {
        selectedCards.Clear();
        UpdateUI();
    }
}