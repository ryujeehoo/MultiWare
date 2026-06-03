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

        GetComponent<TooltipTrigger>().SetTooltip(relicData.relicName, relicData.description);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnClickBuy);
    }

    void OnClickBuy()
    {
        if (purchased) return;
        if (GoldManager.Instance.SpendGold(price))
        {
            purchased = true; // 맨 먼저 true로 설정
            Player.Instance.relics.Add(relic);
            relic.OnRelicAcquired(Player.Instance, FindAnyObjectByType<DeckManager>());
            AudioManager.Instance.PlayGoldSpend();
            buyButton.interactable = false;
            priceText.text = "Sold out";

            // 패널이 열려있을 때만 갱신
            if (RelicPanelUI.Instance != null && RelicPanelUI.Instance.isOpen)
                RelicPanelUI.Instance.Refresh();
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