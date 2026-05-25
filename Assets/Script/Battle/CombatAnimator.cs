using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CombatAnimator : MonoBehaviour
{
    public static CombatAnimator Instance;

    [Header("Camera Shake")]
    public float shakeIntensity = 0.12f;
    public float shakeDuration = 0.25f;

    [Header("Attack Punch")]
    public float punchDuration = 0.08f;
    public float punchScale = 1.2f;

    private Canvas _canvas;
    private bool _shaking;

    void Awake() => Instance = this;
    void OnDestroy() { if (Instance == this) Instance = null; }

    public Canvas GetOrCreateCanvas()
    {
        if (_canvas != null) return _canvas;
        var go = new GameObject("CombatOverlayCanvas");
        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200;
        go.AddComponent<CanvasScaler>();
        return _canvas;
    }

    public void ShakeScreen()
    {
        if (!_shaking) StartCoroutine(DoShake());
    }

    IEnumerator DoShake()
    {
        _shaking = true;
        Camera cam = Camera.main;
        if (cam == null) { _shaking = false; yield break; }

        Vector3 origin = cam.transform.localPosition;
        for (float t = 0f; t < shakeDuration; t += Time.deltaTime)
        {
            float str = shakeIntensity * (1f - t / shakeDuration);
            Vector2 offset = Random.insideUnitCircle * str;
            cam.transform.localPosition = origin + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }
        cam.transform.localPosition = origin;
        _shaking = false;
    }

    // 몬스터 공격 시 시각적 펀치 효과 (찌그러짐 → 원복)
    public IEnumerator MonsterAttackPunch(Transform visual)
    {
        if (visual == null) yield break;

        Vector3 orig = visual.localScale;
        Vector3 punched = new Vector3(orig.x * punchScale, orig.y * (2f - punchScale), orig.z);

        for (float t = 0f; t < punchDuration; t += Time.deltaTime)
        {
            visual.localScale = Vector3.Lerp(orig, punched, t / punchDuration);
            yield return null;
        }
        for (float t = 0f; t < punchDuration; t += Time.deltaTime)
        {
            visual.localScale = Vector3.Lerp(punched, orig, t / punchDuration);
            yield return null;
        }
        visual.localScale = orig;
    }

    // 플레이어 피격 시 전체화면 적색 비네트
    public void PlayerDamageFlash()
    {
        StartCoroutine(DoPlayerDamageFlash());
    }

    IEnumerator DoPlayerDamageFlash()
    {
        Canvas canvas = GetOrCreateCanvas();
        var go = new GameObject("DamageVignette");
        go.transform.SetParent(canvas.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.raycastTarget = false;
        img.color = new Color(1f, 0f, 0f, 0f);

        float duration = 0.45f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float alpha = Mathf.Sin(t / duration * Mathf.PI) * 0.4f;
            img.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }
        Destroy(go);
    }
}
