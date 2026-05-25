using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    public static void Spawn(Vector3 worldPos, int amount, bool isHeal = false)
    {
        if (amount <= 0) return;

        Canvas canvas = CombatAnimator.Instance != null
            ? CombatAnimator.Instance.GetOrCreateCanvas()
            : CreateFallbackCanvas();
        if (canvas == null) return;

        var go = new GameObject("DmgNum");
        go.transform.SetParent(canvas.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160f, 60f);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = isHeal ? $"+{amount}" : $"{amount}";
        tmp.fontSize = 42;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = isHeal ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.9f, 0.15f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;

        Camera cam = Camera.main;
        Vector2 screenPt = cam != null
            ? (Vector2)cam.WorldToScreenPoint(worldPos)
            : new Vector2(Screen.width * 0.5f, Screen.height * 0.55f);
        screenPt.x += Random.Range(-25f, 25f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(), screenPt, null, out Vector2 local);
        rt.anchoredPosition = local;

        go.AddComponent<DamageNumber>().Run(tmp, rt);
    }

    static Canvas CreateFallbackCanvas()
    {
        var go = new GameObject("DmgCanvas");
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 200;
        go.AddComponent<CanvasScaler>();
        return c;
    }

    void Run(TextMeshProUGUI tmp, RectTransform rt) => StartCoroutine(Animate(tmp, rt));

    IEnumerator Animate(TextMeshProUGUI tmp, RectTransform rt)
    {
        // 팝인 효과
        for (float t = 0f; t < 0.08f; t += Time.deltaTime)
        {
            rt.localScale = Vector3.Lerp(Vector3.one * 1.4f, Vector3.one, t / 0.08f);
            yield return null;
        }
        rt.localScale = Vector3.one;

        // 위로 떠오르며 페이드 아웃
        Vector2 start = rt.anchoredPosition;
        float dur = 1.1f;
        for (float t = 0f; t < dur; t += Time.deltaTime)
        {
            float pct = t / dur;
            rt.anchoredPosition = start + Vector2.up * (70f * pct);
            tmp.alpha = pct < 0.5f ? 1f : 1f - (pct - 0.5f) * 2f;
            yield return null;
        }

        Destroy(gameObject);
    }
}
