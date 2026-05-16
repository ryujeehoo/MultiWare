using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardView : MonoBehaviour, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("카드 데이터")]
    public CardData cardData;

    [Header("UI 연결")]
    public Image cardImage;
    public TMP_Text cardNameText;

    [Header("선택 상태")]
    public bool isSelected = false;
    private RectTransform cardContainer;
    private Vector2 containerOriginalPos;

    // 화살표 관련
    private LineRenderer lineRenderer;
    private bool isDraggingArrow = false;

    private void Update()
    {
        if (isDraggingArrow && lineRenderer != null)
            UpdateArrow(Input.mousePosition);
    }

    public void Setup(CardData data)
    {
        cardData = data;

        // CardContainer 찾기
        Transform container = transform.Find("CardContainer");
        if (container != null)
        {
            cardContainer = container.GetComponent<RectTransform>();
            containerOriginalPos = cardContainer.anchoredPosition;
        }

        // CardData_csj 컴포넌트 찾기 및 설정
        CardData_csj cardVisual = GetComponentInChildren<CardData_csj>();
        if (cardVisual != null)
        {
            cardVisual.suit = ConvertSuit(data.suit);
            cardVisual.number = data.cardNumber;
            cardVisual.SetupCard();
            Debug.Log($"카드 설정 완료: {data.cardName}");
        }
        else
        {
            Debug.Log("CardData_csj 컴포넌트를 찾을 수 없습니다!");
        }

        // cardNameText 비활성화 코드 제거! ← 이 부분 삭제
        // if (cardNameText == null)
        //     cardNameText = GetComponentInChildren<TMP_Text>();
        // if (cardNameText != null)
        //     cardNameText.gameObject.SetActive(false);
    }
    // SuitType → Suit 변환
    private Suit ConvertSuit(SuitType suitType)
    {
        switch (suitType)
        {
            case SuitType.Spade: return Suit.Spade;
            case SuitType.Heart: return Suit.Heart;
            case SuitType.Diamond: return Suit.Diamond;
            case SuitType.Clover: return Suit.Club;
            default: return Suit.Spade;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            return;

        // 리롤 모드일 때 → 리롤 선택
        if (CardRerollManager.Instance != null &&
            CardRerollManager.Instance.isRerollMode)
        {
            ToggleSelect();
            CardRerollManager.Instance.ToggleRerollSelect(this);
        }
        // 일반 모드일 때 → 전투 선택
        else
        {
            ToggleSelect();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isSelected) return;

        List<CardView> selected = CardSelectManager.Instance.GetSelectedCards();
        if (selected.Count == 0) return;

        isDraggingArrow = true;
        CreateArrow();
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggingArrow) return;
        isDraggingArrow = false;

        DestroyArrow();

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // RerollZone 감지
            RerollZone rerollZone = result.gameObject.GetComponent<RerollZone>();
            if (rerollZone != null)
            {
                List<CardView> selected = CardSelectManager.Instance.GetSelectedCards();
                int rerollCount = selected.Count;
                int totalCost = CardRerollManager.Instance != null ?
                    CardRerollManager.Instance.rerollCost * rerollCount : 5 * rerollCount;

                if (GoldManager.Instance.currentGold < totalCost)
                {
                    Debug.Log($"골드 부족! 필요: {totalCost}G");
                    return;
                }

                GoldManager.Instance.SpendGold(totalCost);
                Debug.Log($"리롤! -{totalCost}G / {rerollCount}장 교체");

                // 카드 제거 전에 rerollCount 저장
                List<CardView> cardsToRemove = new List<CardView>(selected);
                CardSelectManager.Instance.ClearSelected();

                // 카드 먼저 제거
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

                // TurnManager에서 코루틴 실행 (CardView가 Destroy되기 전에!)
                if (CardDealAnimator.Instance != null && TurnManager.Instance != null)
                    TurnManager.Instance.StartCoroutine(
                        CardDealAnimator.Instance.DealCardsAnimation(rerollCount));

                return;
            }

            // 몬스터 감지
            Monster monster = result.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                List<CardView> selected = CardSelectManager.Instance.GetSelectedCards();
                List<CardData> cardDataList = new List<CardData>();
                foreach (CardView cv in selected)
                    cardDataList.Add(cv.cardData);

                HandResult handResult = HandEvaluator.Evaluate(cardDataList);
                Debug.Log($"족보: {handResult.GetRankName()} / 데미지: {handResult.baseDamage}");

                monster.ReceiveCardBundle(handResult, cardDataList);

                List<CardView> cardsToRemove = new List<CardView>(selected);
                CardSelectManager.Instance.ClearSelected();

                foreach (CardView cv in cardsToRemove)
                {
                    if (cv != null && cv.gameObject != null)
                        Destroy(cv.gameObject);
                }

                return;
            }
        }

        Debug.Log("몬스터 또는 리롤존을 타겟으로 지정하세요!");
    }

    // 화살표 생성
    private void CreateArrow()
    {
        GameObject lineObj = new GameObject("ArrowLine");
        lineRenderer = lineObj.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = 999;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        Debug.Log("화살표 생성됨!");
    }

    // 화살표 업데이트
    private void UpdateArrow(Vector2 endPos)
    {
        if (lineRenderer == null) return;

        Camera cam = Camera.main;

        Vector3 startWorld = cam.ScreenToWorldPoint(
            new Vector3(
                GetComponent<RectTransform>().position.x,
                GetComponent<RectTransform>().position.y,
                cam.nearClipPlane + 1f));

        Vector3 endWorld = cam.ScreenToWorldPoint(
            new Vector3(endPos.x, endPos.y, cam.nearClipPlane + 1f));

        lineRenderer.SetPosition(0, startWorld);
        lineRenderer.SetPosition(1, endWorld);
    }

    // 화살표 제거
    private void DestroyArrow()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer.gameObject);
            lineRenderer = null;
        }
    }

    // 카드 선택/해제
    public void ToggleSelect()
    {
        isSelected = !isSelected;

        if (cardContainer == null) return;

        if (isSelected)
        {
            cardContainer.anchoredPosition = new Vector2(
                containerOriginalPos.x,
                containerOriginalPos.y + 30);
            if (CardSelectManager.Instance != null)
                CardSelectManager.Instance.SelectCard(this);
            Debug.Log($"{cardData.cardName} 선택됨");
        }
        else
        {
            cardContainer.anchoredPosition = containerOriginalPos;
            if (CardSelectManager.Instance != null)
                CardSelectManager.Instance.DeselectCard(this);
            Debug.Log($"{cardData.cardName} 선택 해제");
        }
    }

    public void UseCard()
    {
        if (cardData == null) return;

        switch (cardData.suit)
        {
            case SuitType.Spade:
                Debug.Log($"공격! {cardData.attackPower} 데미지");
                break;
            case SuitType.Heart:
                Debug.Log($"회복! {cardData.healAmount} HP 회복");
                break;
            case SuitType.Diamond:
                Debug.Log($"골드 {cardData.goldAmount} 획득");
                break;
            case SuitType.Clover:
                Debug.Log($"버프/디버프 {cardData.buffValue} 적용");
                break;
        }

        HandManager.Instance.RemoveCardFromHand(this.gameObject);
    }

    public SuitType GetSuit() { return cardData.suit; }
    public int GetAttack() { return cardData.attackPower; }
    public int GetHeal() { return cardData.healAmount; }
    public int GetGold() { return cardData.goldAmount; }
    public int GetBuff() { return cardData.buffValue; }
}