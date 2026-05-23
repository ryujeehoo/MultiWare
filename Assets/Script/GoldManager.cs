using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;
    private float goldMultiplier = 1.0f;
    public float diamondGoldMultiplier = 1.0f;

    [Header("골드 설정")]
    public int currentGold = 0;

    [Header("UI 연결")]
    public TMP_Text goldText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 씬 로드될 때마다 자동으로 goldText 재연결
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새 씬에서 goldText 오브젝트 찾아서 재연결
        GameObject goldUI = GameObject.Find("gold");
        if (goldUI != null)
            goldText = goldUI.GetComponent<TMP_Text>();

        UpdateGoldUI();  // 재연결 후 즉시 UI 갱신
    }

    private void Start()
    {
        currentGold = 30;
        UpdateGoldUI();
    }

    public void AddGoldMultiplier(float amount)
    {
        goldMultiplier += amount;
    }

    public void AddGold(int amount)
    {
        int finalGold = Mathf.RoundToInt(amount * goldMultiplier);
        currentGold += finalGold;
        UpdateGoldUI();
    }

    public bool SpendGold(int amount)
    {
        if (currentGold < amount)
        {
            Debug.Log("골드가 부족합니다!");
            return false;
        }
        currentGold -= amount;
        UpdateGoldUI();
        return true;
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = $"gold: {currentGold}";
    }
}