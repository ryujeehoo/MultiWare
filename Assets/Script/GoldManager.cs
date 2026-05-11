using UnityEngine;
using TMPro;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;
    private float goldMultiplier = 1.0f;

    [Header("∞ÒµÂ º≥¡§")]
    public int currentGold = 0;

    [Header("UI ø¨∞·")]
    public TMP_Text goldText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentGold = 30;  // °Á Ω√¿€ ∞ÒµÂ 30G
        UpdateGoldUI();
        Debug.Log($"Ω√¿€ ∞ÒµÂ: {currentGold}G");
    }
    public void AddGoldMultiplier(float amount)
    {
        goldMultiplier += amount;
        Debug.Log($"∞ÒµÂ πË¿≤ √ﬂ∞°! «ˆ¿Á πË¿≤: {goldMultiplier}");
    }
    // ∞ÒµÂ »πµÊ
    public void AddGold(int amount)
    {
        int finalGold = Mathf.RoundToInt(amount * goldMultiplier);
        currentGold += finalGold;
        UpdateGoldUI();
        Debug.Log($"∞ÒµÂ »πµÊ! +{finalGold} / «ˆ¿Á ∞ÒµÂ: {currentGold}");
    }

    // ∞ÒµÂ ªÁøÎ
    public bool SpendGold(int amount)
    {
        if (currentGold < amount)
        {
            Debug.Log("∞ÒµÂ∞° ∫Œ¡∑«’¥œ¥Ÿ!");
            return false;
        }
        currentGold -= amount;
        UpdateGoldUI();
        Debug.Log($"∞ÒµÂ ªÁøÎ! -{amount} / «ˆ¿Á ∞ÒµÂ: {currentGold}");
        return true;
    }

    // UI æ˜µ•¿Ã∆Æ
    private void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = $"gold: {currentGold}";
    }
}