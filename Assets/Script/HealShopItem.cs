using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealShopItem : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI healAmountText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private int healAmount;
    private int price;
    private bool purchased = false;

    public void Setup(int heal, int cost)
    {
        healAmount = heal;
        price = cost;

        healAmountText.text = $"HP +{heal}";
        priceText.text = $"{cost} G";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnClickBuy);
    }

    void OnClickBuy()
    {
        if (purchased) return;

        if (GoldManager.Instance.SpendGold(price))
        {
            Player.Instance.Heal(healAmount);
            purchased = true;
            AudioManager.Instance.PlayGoldSpend();
            buyButton.interactable = false;
            priceText.text = "Sold Out";
            Debug.Log($"[상점] 체력 {healAmount} 회복!");

        }
        else
        {
            Debug.Log("[상점] 골드 부족!");
            StartCoroutine(FlashPrice());
        }
    }

    System.Collections.IEnumerator FlashPrice()
    {
        priceText.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        ColorUtility.TryParseHtmlString("#E9BE3F", out Color color);
        priceText.color = color;
    }
}