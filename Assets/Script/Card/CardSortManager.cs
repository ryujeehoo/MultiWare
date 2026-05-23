using System.Collections.Generic;
using UnityEngine;

public class CardSortManager : MonoBehaviour
{
    public static CardSortManager Instance;
    public CardFan cardFan;

    private void Awake()
    {
        Instance = this;
    }

    // 문양별 정렬 버튼 클릭
    public void OnClickSortBySuit()
    {
        List<GameObject> cards = cardFan.GetCardList();
        if (cards == null || cards.Count == 0) return;

        cards.Sort((a, b) =>
        {
            CardView cardA = a.GetComponent<CardView>();
            CardView cardB = b.GetComponent<CardView>();

            if (cardA == null || cardB == null) return 0;

            int suitA = GetSuitOrder(cardA.cardData.suit);
            int suitB = GetSuitOrder(cardB.cardData.suit);

            // 문양이 같으면 숫자로 정렬
            if (suitA == suitB)
                return cardA.cardData.cardNumber.CompareTo(cardB.cardData.cardNumber);

            return suitA.CompareTo(suitB);
        });

        cardFan.SetCardList(cards);
        Debug.Log("문양별 정렬 완료!");
    }

    // 숫자순 정렬 버튼 클릭
    public void OnClickSortByNumber()
    {
        List<GameObject> cards = cardFan.GetCardList();
        if (cards == null || cards.Count == 0) return;

        cards.Sort((a, b) =>
        {
            CardView cardA = a.GetComponent<CardView>();
            CardView cardB = b.GetComponent<CardView>();

            if (cardA == null || cardB == null) return 0;

            int numA = cardA.cardData.cardNumber == 1 ||
                       cardA.cardData.cardNumber == 14 ? 14 : cardA.cardData.cardNumber;
            int numB = cardB.cardData.cardNumber == 1 ||
                       cardB.cardData.cardNumber == 14 ? 14 : cardB.cardData.cardNumber;

            // 숫자가 같으면 문양으로 정렬
            if (numA == numB)
                return GetSuitOrder(cardA.cardData.suit)
                    .CompareTo(GetSuitOrder(cardB.cardData.suit));

            return numA.CompareTo(numB);
        });

        cardFan.SetCardList(cards);
        Debug.Log("숫자순 정렬 완료!");
    }

    // 문양 순서 (스페이드→다이아→하트→클로버)
    private int GetSuitOrder(SuitType suit)
    {
        switch (suit)
        {
            case SuitType.Spade: return 0;
            case SuitType.Diamond: return 1;
            case SuitType.Heart: return 2;
            case SuitType.Clover: return 3;
            default: return 4;
        }
    }
}