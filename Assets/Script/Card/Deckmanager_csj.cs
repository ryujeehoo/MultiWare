using System.Collections.Generic;
using UnityEngine;

public class DeckManager_csj : MonoBehaviour
{
    public GameObject cardPrefab; // Card 프리팹 연결
    public Transform deckParent;  // 카드 담을 부모 오브젝트

    private List<CardData_csj> deck = new List<CardData_csj>();

    void Start()
    {
        CreateDeck();
        Shuffle();
    }

    void CreateDeck()
    {
        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            for (int i = 1; i <= 13; i++)
            {
                GameObject cardObj = Instantiate(cardPrefab, deckParent);
                CardData_csj card = cardObj.GetComponent<CardData_csj>();
                card.suit = suit;
                card.number = i;
                card.SetupCard();
                deck.Add(card);
            }
        }
        Debug.Log($"덱 생성 완료: {deck.Count}장");
    }

    void Shuffle()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (deck[i], deck[rand]) = (deck[rand], deck[i]);
        }
    }

    public CardData_csj DrawCard()
    {
        if (deck.Count == 0) return null;
        CardData_csj card = deck[0];
        deck.RemoveAt(0);
        return card;
    }
}