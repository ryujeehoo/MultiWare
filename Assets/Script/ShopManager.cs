using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("유물 풀")]
    public List<Relic> allRelics;

    [Header("상점 슬롯")]
    public List<RelicShopItem> shopSlots;

    [Header("가격 범위")]
    public int minPrice = 50;
    public int maxPrice = 150;

    [Header("UI")]
    public Button continueButton;
    [Header("카드 삭제 슬롯")]
    public CardDeleteShopItem cardDeleteSlot;
    public int cardDeletePrice = 80;
    [Header("체력 회복 슬롯")]
    public HealShopItem healSlot;
    public int healAmount = 30;
    public int healPrice = 50;
    void Start()
    {
        AudioManager.Instance.PlayShopEnter();
        GenerateShop();
        SetupHealSlot();
        continueButton.onClick.AddListener(OnClickContinue);
        cardDeleteSlot.Setup(cardDeletePrice);
    }

    void GenerateShop()
    {
        List<Relic> available = allRelics.FindAll(r => !Player.Instance.relics.Contains(r));

        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        for (int i = 0; i < shopSlots.Count; i++)
        {
            if (i < available.Count)
            {
                int price = Random.Range(minPrice, maxPrice + 1);
                price = Mathf.RoundToInt(price / 10f) * 10;
                shopSlots[i].gameObject.SetActive(true);
                shopSlots[i].Setup(available[i], price);
            }
            else
            {
                shopSlots[i].gameObject.SetActive(false);
            }
        }
    }
    void SetupHealSlot()
    {
        if (healSlot != null)
            healSlot.Setup(healAmount, healPrice);
    }
    void OnClickContinue()
    {
        GameManager.Instance.OnShopDone();
    }
}