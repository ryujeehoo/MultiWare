using System.Collections.Generic;
using UnityEngine;

public class CardFan : MonoBehaviour
{
    public int cardMax = 5;

    [Header("Card Settings")]
    public GameObject cardPrefab;
    public int cardCount = 5;
    public float spacing = 150f;
    public float radius = 5000f;
    public float maxAngle = 30f;

    private GameObject[] cards;

    void Start()
    {
        //CreateCards(cardCount);
    }

    void CreateCards(int count)
    {
        if (cards != null)
        {
            foreach (var c in cards)
                if (c != null) Destroy(c.gameObject);
        }

        cards = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            GameObject card = Instantiate(cardPrefab, transform);
            cards[i] = card;

            CardDrag cardDrag = card.GetComponent<CardDrag>();
            if (cardDrag != null)
                cardDrag.SetDraggable(false);

            CardView cardView = card.GetComponent<CardView>();
            if (cardView != null)
            {
                CardData cardData = DeckManager.Instance.DrawCard();
                if (cardData != null)
                    cardView.Setup(cardData);
            }

            // CardHover 추가 제거! ← 이 부분 삭제
        }
        ArrangeCards();
    }

    void ArrangeCards()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null) continue;

            RectTransform cardTransform = cards[i].GetComponent<RectTransform>();
            if (cardTransform == null) continue;

            // 1자 배치 - spacing만큼 가로로 배치
            float normPos = cards.Length == 1 ? 0f :
                (i / (float)(cards.Length - 1)) - 0.5f;
            float x = spacing * normPos * cards.Length;

            cardTransform.anchoredPosition = new Vector2(x, 0); // y는 0 고정
            cardTransform.rotation = Quaternion.identity; // 회전 없음
        }
    }
    [ContextMenu("Draw Card")]
    public void DrawCard()
    {
        cardCount++;
        CreateCards(cardCount);
    }

    [ContextMenu("Remove Card")]
    public void RemoveCard()
    {
        if (cardCount <= 1) return;
        cardCount--;
        CreateCards(cardCount);
    }

    public void RemoveCardObject(GameObject card)
    {
        if (cards == null) return;

        List<GameObject> list = new List<GameObject>(cards);
        list.Remove(card);

        // null 제거 추가!
        list.RemoveAll(c => c == null);

        cards = list.ToArray();
        cardCount = cards.Length;
        ArrangeCards();
    }

    public void DrawStartingHand()
    {
        cardCount = 8;  // ← 7 → 8로 변경
        CreateCards(cardCount);
    }
    // 현재 남아있는 카드 수 반환
    public int GetCurrentCardCount()
    {
        if (cards == null) return 0;
        int count = 0;
        foreach (var c in cards)
            if (c != null) count++;
        return count;
    }

    // 카드 추가
    public void AddCard(CardData cardData)
    {
        GameObject card = Instantiate(cardPrefab, transform);

        // CardDrag 비활성화
        CardDrag cardDrag = card.GetComponent<CardDrag>();
        if (cardDrag != null)
            cardDrag.SetDraggable(false);

        CardView cardView = card.GetComponent<CardView>();
        if (cardView != null)
            cardView.Setup(cardData);

       

        List<GameObject> cardList = cards != null ?
            new List<GameObject>(cards) : new List<GameObject>();
        cardList.RemoveAll(c => c == null);
        cardList.Add(card);
        cards = cardList.ToArray();
        cardCount = cards.Length;

        ArrangeCards();
    }

    // 카드 전체 제거
    public void ClearCards()
    {
        if (cards != null)
        {
            foreach (var c in cards)
                if (c != null) Destroy(c.gameObject);
        }
        cards = null;
        cardCount = 0;
    }
}