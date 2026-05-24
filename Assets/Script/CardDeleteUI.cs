using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDeleteUI : MonoBehaviour
{
    public static CardDeleteUI Instance;

    [Header("UI 연결")]
    public GameObject panel;
    public Transform spadeRow;
    public Transform heartRow;
    public Transform cloverRow;
    public Transform diamondRow;
    public GameObject cardIconPrefab;     // DeckViewUI랑 같은 프리팹 써도 됨
    public TextMeshProUGUI remainText;
    public Button confirmButton;
    public Button cancelButton;
    [Header("설정")]
    public int maxDeleteCount = 2;

    private List<CardData> selectedCards = new List<CardData>();
    private List<CardDeleteSlot> slots = new List<CardDeleteSlot>();

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        cancelButton.onClick.AddListener(OnClickCancel); // ← 추가
    }
    public void Open()
    {
        panel.SetActive(true);
        selectedCards.Clear();
        slots.Clear();
        GenerateCardList();
        UpdateUI();

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnClickConfirm);
    }

    void GenerateCardList()
    {
        ClearRow(spadeRow);
        ClearRow(heartRow);
        ClearRow(cloverRow);
        ClearRow(diamondRow);

        int[] numberOrder = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };

        foreach (int number in numberOrder)
        {
            AddCardSlot(SuitType.Spade, number, spadeRow);
            AddCardSlot(SuitType.Heart, number, heartRow);
            AddCardSlot(SuitType.Clover, number, cloverRow);
            AddCardSlot(SuitType.Diamond, number, diamondRow);
        }
    }

    void AddCardSlot(SuitType suit, int number, Transform row)
    {
        if (row == null || cardIconPrefab == null) return;

        CardData card = DeckManager.Instance.allCards.Find(
            c => c.suit == suit &&
            (c.cardNumber == number || (number == 1 && c.cardNumber == 14)));

        if (card == null) return;

        GameObject obj = Instantiate(cardIconPrefab, row);
        CardDeleteSlot slot = obj.GetComponent<CardDeleteSlot>();

        // CardDeleteSlot이 없으면 추가
        if (slot == null)
            slot = obj.AddComponent<CardDeleteSlot>();

        slot.Setup(card, this);
        slots.Add(slot);
    }

    public void OnCardSelected(CardData card)
    {
        if (selectedCards.Contains(card))
            selectedCards.Remove(card);
        else
        {
            if (selectedCards.Count >= maxDeleteCount) return;
            selectedCards.Add(card);
        }
        UpdateUI();
    }

    public bool IsSelected(CardData card) => selectedCards.Contains(card);

    void UpdateUI()
    {
        remainText.text = $"{selectedCards.Count} / {maxDeleteCount}";
        confirmButton.interactable = selectedCards.Count > 0;
    }

    void OnClickConfirm()
    {
        foreach (CardData card in selectedCards)
        {
            DeckManager.Instance.RemoveCardBySuitAndNumber(card.suit, card.cardNumber);
            Debug.Log($"[카드 삭제] {card.cardName} 제거!");
        }

        FindAnyObjectByType<CardDeleteShopItem>().CompletePurchase(); // ← 추가
        panel.SetActive(false);
    }
    void OnClickCancel()
    {
        panel.SetActive(false);
        CardDeleteShopItem shopItem = FindAnyObjectByType<CardDeleteShopItem>();
        GoldManager.Instance.AddGold(shopItem.GetPrice());
        shopItem.CancelPurchase();
        Debug.Log("[카드 삭제] 취소! 골드 환불");
    }
    void ClearRow(Transform row)
    {
        if (row == null) return;
        foreach (Transform child in row)
            Destroy(child.gameObject);
    }
}