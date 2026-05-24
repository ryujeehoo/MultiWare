using UnityEngine;
using UnityEngine.UI;

public class CardDeleteSlot : MonoBehaviour
{
    [Header("UI")]
    public GameObject selectedOverlay;

    private CardData cardData;
    private CardDeleteUI deleteUI;

    public void Setup(CardData data, CardDeleteUI ui)
    {
        cardData = data;
        deleteUI = ui;

        // 카드 이미지 적용
        Image img = GetComponent<Image>();
        if (img != null && data.cardSprite != null)
            img.sprite = data.cardSprite;

        // 선택 오버레이 생성 (없으면 자동 생성)
        if (selectedOverlay == null)
        {
            selectedOverlay = new GameObject("SelectedOverlay");
            selectedOverlay.transform.SetParent(transform, false);

            RectTransform rt = selectedOverlay.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image overlayImg = selectedOverlay.AddComponent<Image>();
            overlayImg.color = new Color(0f, 1f, 0f, 0.4f);
        }
        selectedOverlay.SetActive(false);

        // 버튼 이벤트
        Button btn = GetComponent<Button>();
        if (btn == null) btn = gameObject.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        deleteUI.OnCardSelected(cardData);
        selectedOverlay?.SetActive(deleteUI.IsSelected(cardData));
    }
}