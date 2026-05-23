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
        // Player.Instance가 없으면 잠깐 기다림
        player = Player.Instance;

        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealth; // 중복 구독 방지
            player.OnHealthChanged += UpdateHealth;
            UpdateHealth(player.currentHealth, player.maxHealth);
        }
        else
        {
            // Instance가 아직 없으면 1프레임 후 재시도
            StartCoroutine(WaitAndSubscribe());
        }
    }
    private System.Collections.IEnumerator WaitAndSubscribe()
    {
        yield return null; // 1프레임 대기
        player = Player.Instance;
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealth;
            player.OnHealthChanged += UpdateHealth;
            UpdateHealth(player.currentHealth, player.maxHealth);
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