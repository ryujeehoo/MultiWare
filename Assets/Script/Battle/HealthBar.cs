using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Image hpFill;
    public Image hpBar;
    public TextMeshProUGUI healthText;
    public Player player;

    void OnEnable()
    {
        if (player == null)
            player = Player.Instance;

        if (player != null)
        {
            player.OnHealthChanged += UpdateHealth;
            UpdateHealth(player.currentHealth, player.maxHealth); // 즉시 반영
        }
    }

    void OnDisable()
    {
        if (player != null)
            player.OnHealthChanged -= UpdateHealth;
    }

    public void UpdateHealth(float current, float max)
    {
        if (hpFill != null)
            hpFill.fillAmount = current / max;
        if (healthText != null)
            healthText.text = $"{current} / {max}";
    }
}