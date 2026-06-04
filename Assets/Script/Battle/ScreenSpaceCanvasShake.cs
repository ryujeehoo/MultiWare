using System.Collections.Generic;
using UnityEngine;

public class ScreenSpaceCanvasShake : MonoBehaviour
{
    private const float ShakeFrequency = 7f;

    private RectTransform rectTransform;
    private readonly List<RectTransform> targets = new List<RectTransform>();
    private readonly List<Vector2> originPositions = new List<Vector2>();
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

    public static void ShakeAll(float duration, float magnitude)
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                continue;

            ScreenSpaceCanvasShake shake = canvas.GetComponent<ScreenSpaceCanvasShake>();
            if (shake == null)
                shake = canvas.gameObject.AddComponent<ScreenSpaceCanvasShake>();

            shake.Shake(duration, magnitude);
        }
    }

    public void Shake(float duration, float magnitude)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        CaptureTargets();

        totalTime = Mathf.Max(0.01f, duration);
        remainingTime = totalTime;
        this.magnitude = Mathf.Max(0f, magnitude);
        seedX = Random.Range(0f, 1000f);
        seedY = Random.Range(0f, 1000f);
        hasOrigin = true;
    }

    void LateUpdate()
    {
        if (!hasOrigin)
            return;

        if (remainingTime <= 0f)
        {
            RestoreTargets();
            hasOrigin = false;
            return;
        }

        remainingTime -= Time.unscaledDeltaTime;

        float progress = Mathf.Clamp01(remainingTime / totalTime);
        float currentMagnitude = magnitude * progress;
        float time = Time.unscaledTime * ShakeFrequency;
        float x = (Mathf.PerlinNoise(seedX, time) * 2f - 1f) * currentMagnitude;
        float y = (Mathf.PerlinNoise(seedY, time) * 2f - 1f) * currentMagnitude;

        ApplyOffset(new Vector2(x, y));
    }

    private void CaptureTargets()
    {
        RestoreTargets();
        targets.Clear();
        originPositions.Clear();

        RectTransform[] allChildren = GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform target in allChildren)
        {
            if (target == null)
                continue;

            targets.Add(target);
            originPositions.Add(target.anchoredPosition);
        }
    }

    private void ApplyOffset(Vector2 offset)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null) continue;
            targets[i].anchoredPosition = originPositions[i] + offset;
        }
    }

    private void RestoreTargets()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null) continue;
            targets[i].anchoredPosition = originPositions[i];
        }
    }
}
