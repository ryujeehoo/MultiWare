using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HitEffectUI : MonoBehaviour
{
    public static HitEffectUI Instance;

    [Header("Damage vignette overlay")]
    public Image vignetteImage;

    [Header("Monster hit image")]
    public Image hitImage;

    private Coroutine vignetteRoutine;
    private Coroutine hitImageRoutine;

    public static HitEffectUI GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject canvasObject = new GameObject("HitEffectUI Canvas");
        DontDestroyOnLoad(canvasObject);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject vignetteObject = new GameObject("Damage Vignette");
        vignetteObject.transform.SetParent(canvasObject.transform, false);

        Image vignette = vignetteObject.AddComponent<Image>();
        vignette.sprite = Resources.Load<Sprite>("vignette");
        vignette.color = new Color(1f, 1f, 1f, 0f);
        vignette.raycastTarget = false;
        vignette.preserveAspect = false;

        RectTransform rect = vignette.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject hitImageObject = new GameObject("Player Attack Hit Image");
        hitImageObject.transform.SetParent(canvasObject.transform, false);

        Image hitImage = hitImageObject.AddComponent<Image>();
        hitImage.color = new Color(1f, 1f, 1f, 0f);
        hitImage.raycastTarget = false;
        hitImage.preserveAspect = true;

        RectTransform hitRect = hitImage.rectTransform;
        hitRect.anchorMin = new Vector2(0.5f, 0.5f);
        hitRect.anchorMax = new Vector2(0.5f, 0.5f);
        hitRect.pivot = new Vector2(0.5f, 0.5f);
        hitRect.anchoredPosition = Vector2.zero;
        hitRect.sizeDelta = new Vector2(420f, 420f);

        HitEffectUI effectUI = canvasObject.AddComponent<HitEffectUI>();
        effectUI.vignetteImage = vignette;
        effectUI.hitImage = hitImage;

        return effectUI;
    }

    public static void ShowDamageVignetteGlobal()
    {
        GetOrCreate().ShowDamageVignette();
    }

    void Awake()
    {
        Instance = this;

        if (vignetteImage != null)
            SetAlpha(vignetteImage, 0f);

        if (hitImage != null)
            SetAlpha(hitImage, 0f);
    }

    public void Show(Sprite hitSprite = null)
    {
        ShowDamageVignette();
        ShowHitImage(hitSprite);
    }

    public void ShowDamageVignette()
    {
        if (vignetteImage == null) return;

        if (vignetteRoutine != null)
            StopCoroutine(vignetteRoutine);

        vignetteRoutine = StartCoroutine(FlashImage(vignetteImage, 0.85f, 0.08f, 0.12f, 0.35f));
    }

    private void ShowHitImage(Sprite hitSprite)
    {
        if (hitImage == null) return;

        if (hitSprite != null)
        {
            hitImage.sprite = hitSprite;
            hitImage.gameObject.SetActive(true);
        }
        else
        {
            hitImage.gameObject.SetActive(false);
            SetAlpha(hitImage, 0f);
            return;
        }

        if (hitImageRoutine != null)
            StopCoroutine(hitImageRoutine);

        hitImageRoutine = StartCoroutine(FlashImage(hitImage, 1f, 0.08f, 0.12f, 0.35f));
    }

    private IEnumerator FlashImage(Image image, float maxAlpha, float fadeInTime, float holdTime, float fadeOutTime)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeInTime;
            SetAlpha(image, Mathf.Lerp(0f, maxAlpha, Mathf.Clamp01(t)));
            yield return null;
        }

        yield return new WaitForSeconds(holdTime);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeOutTime;
            SetAlpha(image, Mathf.Lerp(maxAlpha, 0f, Mathf.Clamp01(t)));
            yield return null;
        }

        SetAlpha(image, 0f);

        if (image == vignetteImage)
            vignetteRoutine = null;
        else if (image == hitImage)
            hitImageRoutine = null;
    }

    private void SetAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
