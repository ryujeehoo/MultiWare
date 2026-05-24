using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDeleteShopItem : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public int GetPrice() => price;
    private int price;
    private bool purchased = false;

    public void Setup(int cost)
    {
        price = cost;
        priceText.text = $"{cost} G";
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnClickBuy);
    }

    void OnClickBuy()
    {
        if (purchased) return;

        if (!GoldManager.Instance.SpendGold(price))
        {
            StartCoroutine(FlashPrice());
            return;
        }

        // Sold out은 패널 닫힐 때 표시
        purchased = true;
        buyButton.interactable = false;
        CardDeleteUI.Instance.Open(); // ← 먼저 열고
    }
    public void CancelPurchase()
    {
        purchased = false;
        buyButton.interactable = true;
        priceText.text = $"{price} G";
    }
    public void CompletePurchase()
    {
        priceText.text = "Sold out"; // ← 확인 눌렀을 때 표시
    }
    System.Collections.IEnumerator FlashPrice()
    {
        priceText.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        ColorUtility.TryParseHtmlString("#E9BE3F", out Color color);
        priceText.color = color;
    }
}