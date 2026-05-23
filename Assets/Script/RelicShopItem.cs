using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelicShopItem : MonoBehaviour
{
    [Header("UI")]
    public Image relicIcon;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private Relic relic;
    private int price;
    private bool purchased = false;

    public void Setup(Relic relicData, int cost)
    {
        relic = relicData;
        price = cost;

        relicIcon.sprite = relicData.icon;
        priceText.text = $"{cost} G";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnClickBuy);
    }

    void OnClickBuy()
    {
        if (purchased) return;

        if (GoldManager.Instance.SpendGold(price))
        {
            Player.Instance.relics.Add(relic);
            relic.OnRelicAcquired(Player.Instance, FindAnyObjectByType<DeckManager>());
            purchased = true;

            buyButton.interactable = false;
            priceText.text = "Sold out";
        }
        else
        {
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