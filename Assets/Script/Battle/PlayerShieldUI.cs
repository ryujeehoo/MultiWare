using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerShieldUI : MonoBehaviour
{
    public static PlayerShieldUI Instance;

    [Header("UI 연결")]
    public GameObject shieldPanel;  // 방어막 패널 (아이콘 + 텍스트)
    public Image shieldIcon;        // 방패 아이콘
    public TMP_Text shieldText;     // 방어막 수치 텍스트

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (Player.Instance != null)
            Player.Instance.OnDefenseChanged += UpdateShieldUI;

        // 처음엔 숨기기
        if (shieldPanel != null)
            shieldPanel.SetActive(false);
    }

    public void UpdateShieldUI(int defense)
    {
        if (shieldPanel != null)
            shieldPanel.SetActive(defense > 0);

        if (shieldText != null)
            shieldText.text = defense.ToString();

        Debug.Log($"방어막 UI 업데이트: {defense}");
    }
}