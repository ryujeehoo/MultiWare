using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardDealAnimator : MonoBehaviour
{
    public static CardDealAnimator Instance;

    [Header("애니메이션 설정")]
    public GameObject cardBackPrefab;
    public Transform deckPosition;
    public Transform handArea;
    public float dealSpeed = 0.3f;
    public float dealDelay = 0.2f;

    [Header("CardFan 연결")]
    public CardFan cardFan;  // ← 추가

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator DealCardsAnimation(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(DealOneCard());
            yield return new WaitForSeconds(dealDelay);
        }
    }

    private IEnumerator DealOneCard()
    {
        // 카드 뽑는 소리 재생!
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCardDraw();

        GameObject cardBack = Instantiate(cardBackPrefab, deckPosition);
        RectTransform cardRect = cardBack.GetComponent<RectTransform>();
        cardRect.anchoredPosition = Vector2.zero;

        Vector2 startPos = deckPosition.position;
        Vector2 endPos = handArea.position;
        float elapsed = 0f;

        while (elapsed < dealSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dealSpeed;
            t = 1 - Mathf.Pow(1 - t, 3);
            cardBack.transform.position = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        Destroy(cardBack);

        if (DeckManager.Instance != null)
        {
            CardData card = DeckManager.Instance.DrawCard();
            if (HandManager.Instance != null)
            {
                HandManager.Instance.AddCardToHand(card);
            }
            else if (cardFan != null)
            {
                cardFan.AddCard(card);
            }
        }
    }
}