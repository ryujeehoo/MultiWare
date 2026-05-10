using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Image hpFill;          // HP_fill 이미지
    public Image hpBar;           // HP_bar 배경
    public TextMeshProUGUI healthText;
    public Player player;

    void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (player != null)
        {
            player.OnHealthChanged += UpdateHealth;
            UpdateHealth(player.currentHealth, player.maxHealth);
        }
    }

    public void UpdateHealth(float current, float max)
    {
        // HP fill 비율 업데이트
        if (hpFill != null)
            hpFill.fillAmount = current / max;

        if (healthText != null)
            healthText.text = $"{current} / {max}";
    }
}