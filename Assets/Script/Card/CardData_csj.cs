using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum Suit { Spade, Heart, Diamond, Club }

public class CardData_csj : MonoBehaviour
{
    public Suit suit;
    public int number; // 1=A, 11=J, 12=Q, 13=K

    [Header("UI 연결")]
    public Image suitCenter;
    public Image suitTopLeft;
    public Image suitBottomRight;
    public TextMeshProUGUI numberTopLeft;
    public TextMeshProUGUI numberBottomRight;

    [Header("문양 스프라이트")]
    public Sprite spadeSprite;
    public Sprite diamondSprite;
    public Sprite heartSprite;
    public Sprite clubSprite;

    void Start()
    {
       // SetupCard();
    }

    public void SetupCard()
    {
        // 숫자 텍스트 설정
        string numText = GetNumberText();
        numberTopLeft.text = numText;
        numberBottomRight.text = numText;

        // 문양 색상 설정 (검정/빨강)
        Color suitColor = (suit == Suit.Heart || suit == Suit.Diamond)
            ? Color.red : Color.black;
        numberTopLeft.color = suitColor;
        numberBottomRight.color = suitColor;

        // 문양 스프라이트 설정
        Sprite s = GetSuitSprite();
        suitCenter.sprite = s;
        suitTopLeft.sprite = s;
        suitBottomRight.sprite = s;
    }

    string GetNumberText()
    {
        return number switch
        {
            1 => "A",
            14 => "A",  // ← 14도 A로 표시 추가!
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => number.ToString()
        };
    }

    Sprite GetSuitSprite()
    {
        return suit switch
        {
            Suit.Spade => spadeSprite,
            Suit.Diamond => diamondSprite,
            Suit.Heart => heartSprite,
            Suit.Club => clubSprite,
            _ => spadeSprite
        };
    }
}