using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectSlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI stackText;

    public void Setup(Sprite sprite, int stack)
    {
        if (sprite != null)
        {
            icon.sprite = sprite;
            icon.color = Color.white;
        }
        else
        {
            icon.sprite = null;
            icon.color = new Color(1.0f, 1.0f, 1.0f, 1f); 
        }
        stackText.text = stack > 0 ? stack.ToString() : "";
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}