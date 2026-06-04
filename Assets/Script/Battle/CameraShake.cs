using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private const float CanvasShakeScale = 220f;
    private const float ShakeFrequency = 9f;
    private Vector3 originLocalPosition;
    private float remainingTime;
    private float totalTime;
    private float magnitude;
    private float seedX;
    private float seedY;
    private bool hasOrigin;

    void Awake()
    {
        Instance = this;
    }

    public static CameraShake GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        Camera camera = Camera.main;
        if (camera == null)
            camera = FindFirstObjectByType<Camera>();

        if (camera == null)
            return null;

        Instance = camera.GetComponent<CameraShake>();
        if (Instance == null)
            Instance = camera.gameObject.AddComponent<CameraShake>();

        return Instance;
    }

    public static void ShakeAll(float duration, float magnitude)
    {
        CameraShake primary = GetOrCreate();
        if (primary != null)
            primary.Shake(duration, magnitude);

        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera camera in cameras)
        {
            if (camera == null) continue;

            CameraShake shake = camera.GetComponent<CameraShake>();
            if (shake == null)
                shake = camera.gameObject.AddComponent<CameraShake>();

            if (shake != primary)
                shake.Shake(duration, magnitude);
        }

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                continue;

            ScreenSpaceCanvasShake shake = canvas.GetComponent<ScreenSpaceCanvasShake>();
            if (shake == null)
                shake = canvas.gameObject.AddComponent<ScreenSpaceCanvasShake>();

            shake.Shake(duration, magnitude * CanvasShakeScale);
        }
    }

    public void Shake(float duration = 0.3f, float magnitude = 0.2f)
    {
        if (!hasOrigin)
        {
            originLocalPosition = transform.localPosition;
            hasOrigin = true;
        }

        totalTime = Mathf.Max(0.01f, duration);
        remainingTime = totalTime;
        this.magnitude = Mathf.Max(0f, magnitude);
        seedX = Random.Range(0f, 1000f);
        seedY = Random.Range(0f, 1000f);
    }

    void LateUpdate()
    {
        if (remainingTime <= 0f)
        {
            if (hasOrigin)
            {
                transform.localPosition = originLocalPosition;
                hasOrigin = false;
            }
            return;
        }

        remainingTime -= Time.unscaledDeltaTime;

        float progress = Mathf.Clamp01(remainingTime / totalTime);
        float currentMagnitude = magnitude * progress;
        float time = Time.unscaledTime * ShakeFrequency;
        float x = (Mathf.PerlinNoise(seedX, time) * 2f - 1f) * currentMagnitude;
        float y = (Mathf.PerlinNoise(seedY, time) * 2f - 1f) * currentMagnitude;

        transform.localPosition = originLocalPosition + new Vector3(x, y, 0f);
    }
}

public class ScreenSpaceCanvasShake : MonoBehaviour
{
    private const float ShakeFrequency = 9f;
    private RectTransform rectTransform;
    private readonly List<RectTransform> childTargets = new List<RectTransform>();
    private readonly List<Vector2> childOriginPositions = new List<Vector2>();
    private Vector2 originAnchoredPosition;
    private float remainingTime;
    private float totalTime;
    private float magnitude;
    private float seedX;
    private float seedY;
    private bool hasOrigin;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Shake(float duration, float magnitude)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        if (!hasOrigin)
        {
            originAnchoredPosition = rectTransform.anchoredPosition;
            CaptureChildOrigins();
            hasOrigin = true;
        }

        totalTime = Mathf.Max(0.01f, duration);
        remainingTime = totalTime;
        this.magnitude = Mathf.Max(0f, magnitude);
        seedX = Random.Range(0f, 1000f);
        seedY = Random.Range(0f, 1000f);
    }

    void LateUpdate()
    {
        if (rectTransform == null)
            return;

        if (remainingTime <= 0f)
        {
            if (hasOrigin)
            {
                rectTransform.anchoredPosition = originAnchoredPosition;
                RestoreChildOrigins();
                hasOrigin = false;
            }
            return;
        }

        remainingTime -= Time.unscaledDeltaTime;

        float progress = Mathf.Clamp01(remainingTime / totalTime);
        float currentMagnitude = magnitude * progress;
        float time = Time.unscaledTime * ShakeFrequency;
        float x = (Mathf.PerlinNoise(seedX, time) * 2f - 1f) * currentMagnitude;
        float y = (Mathf.PerlinNoise(seedY, time) * 2f - 1f) * currentMagnitude;

        rectTransform.anchoredPosition = originAnchoredPosition + new Vector2(x, y);
        ApplyChildOffset(new Vector2(x, y));
    }

    private void CaptureChildOrigins()
    {
        childTargets.Clear();
        childOriginPositions.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null) continue;

            childTargets.Add(child);
            childOriginPositions.Add(child.anchoredPosition);
        }
    }

    private void ApplyChildOffset(Vector2 offset)
    {
        for (int i = 0; i < childTargets.Count; i++)
        {
            if (childTargets[i] == null) continue;
            childTargets[i].anchoredPosition = childOriginPositions[i] + offset;
        }
    }

    private void RestoreChildOrigins()
    {
        for (int i = 0; i < childTargets.Count; i++)
        {
            if (childTargets[i] == null) continue;
            childTargets[i].anchoredPosition = childOriginPositions[i];
        }

        childTargets.Clear();
        childOriginPositions.Clear();
    }
}
