using UnityEngine;
using UnityEditor;

public class CardSpriteAssigner : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("이미지 폴더 경로")]
    public string spriteFolderPath = "Assets/PlayingCards/Details/2Classic";

    [Header("CardData 폴더 경로")]
    public string cardDataFolderPath = "Assets/Script/Card";

    [ContextMenu("Assign Sprites")]
    public void AssignSprites()
    {
        // 모든 CardData 찾기
        string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { cardDataFolderPath });

        int successCount = 0;
        int failCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardData cardData = AssetDatabase.LoadAssetAtPath<CardData>(path);

            if (cardData == null) continue;

            // 파일명 생성 (예: Spade_A, Club_10)
            string suitName = GetSuitName(cardData.suit);
            string numberName = GetNumberName(cardData.cardNumber);
            string spriteName = $"{suitName}_{numberName}";

            // 스프라이트 찾기
            string spritePath = $"{spriteFolderPath}/{spriteName}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            if (sprite != null)
            {
                cardData.cardSprite = sprite;
                EditorUtility.SetDirty(cardData);
                successCount++;
                Debug.Log($"연결 완료: {cardData.cardName} → {spriteName}");
            }
            else
            {
                failCount++;
                Debug.LogWarning($"스프라이트 없음: {spriteName}");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"완료! 성공: {successCount}장 / 실패: {failCount}장");
    }

    string GetSuitName(SuitType suit)
    {
        switch (suit)
        {
            case SuitType.Spade: return "Spade";
            case SuitType.Heart: return "Heart";
            case SuitType.Diamond: return "Diamond";
            case SuitType.Clover: return "Club";
            default: return "Spade";
        }
    }

    string GetNumberName(int number)
    {
        switch (number)
        {
            case 14: return "A";
            case 11: return "J";
            case 12: return "Q";
            case 13: return "K";
            default: return number.ToString();
        }
    }
#endif
}