using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerAttackVFX
{
    private const int SortingOrder = 70;
    private const float PixelsPerUnit = 256f;

    private static PlayerAttackVFXRunner runner;
    private static readonly Dictionary<string, Sprite> resourceSpriteCache = new Dictionary<string, Sprite>();
    private static readonly HandRankVFXSetting[] rankSettings = CreateDefaultSettings();

    public static void Play(HandRankType rank, Vector3 targetPosition)
    {
        PlayerAttackVFXRunner vfxRunner = GetRunner();
        if (vfxRunner == null)
            return;

        vfxRunner.StartCoroutine(PlayRoutine(rank, targetPosition));
    }

    private static PlayerAttackVFXRunner GetRunner()
    {
        if (runner != null)
            return runner;

        GameObject obj = new GameObject("PlayerAttackVFX");
        Object.DontDestroyOnLoad(obj);
        runner = obj.AddComponent<PlayerAttackVFXRunner>();
        return runner;
    }

    private static IEnumerator PlayRoutine(HandRankType rank, Vector3 target)
    {
        HandRankVFXSetting setting = GetSetting(rank);

        PlayConfiguredPrefab(setting, target);
        PlayCameraShake(rank);

        if (setting != null)
            yield return PlayConfiguredSprites(setting, target);
    }

    private static void PlayConfiguredPrefab(HandRankVFXSetting setting, Vector3 target)
    {
        if (setting == null || setting.prefab == null)
            return;

        GameObject spawned = Object.Instantiate(setting.prefab, target, Quaternion.identity);
        spawned.transform.localScale *= Mathf.Max(0.1f, setting.scale);

        if (setting.prefabLifetime > 0f)
            Object.Destroy(spawned, setting.prefabLifetime);
    }

    private static IEnumerator PlayConfiguredSprites(HandRankVFXSetting setting, Vector3 target)
    {
        if (setting.worldSprites != null)
        {
            foreach (Sprite sprite in setting.worldSprites)
            {
                if (sprite == null) continue;

                yield return PlayWorldSprite(sprite, setting, target, setting.worldOffset);
            }
        }

        if (setting.resourceSpriteNames != null)
        {
            for (int i = 0; i < setting.resourceSpriteNames.Length; i++)
            {
                string resourceName = setting.resourceSpriteNames[i];
                Sprite sprite = LoadAttackSprite(resourceName);
                if (sprite == null) continue;

                Vector2 offset = GetResourceOffset(setting, i);
                yield return PlayWorldSprite(sprite, setting, target, offset);
            }
        }
    }

    private static Vector2 GetResourceOffset(HandRankVFXSetting setting, int index)
    {
        if (setting.resourceOffsets != null &&
            index >= 0 &&
            index < setting.resourceOffsets.Length)
            return setting.resourceOffsets[index];

        return setting.worldOffset;
    }

    private static IEnumerator PlayWorldSprite(Sprite sprite, HandRankVFXSetting setting, Vector3 target, Vector2 offset)
    {
        GameObject obj = new GameObject($"Player Attack VFX - {setting.rank}");
        obj.transform.position = target + (Vector3)offset;
        obj.transform.localScale = Vector3.one * Mathf.Max(0.1f, setting.scale);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1f, 1f, 1f, 0f);
        sr.sortingOrder = SortingOrder;

        float duration = Mathf.Max(0.08f, setting.duration);
        float t = 0f;
        Vector3 startScale = obj.transform.localScale * 0.88f;
        Vector3 endScale = obj.transform.localScale * 1.08f;
        Vector3 startPosition = obj.transform.position;
        Vector3 endPosition = target;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float clamped = Mathf.Clamp01(t);
            float alpha = Mathf.Sin(clamped * Mathf.PI);

            obj.transform.position = Vector3.Lerp(startPosition, endPosition, clamped * 0.35f);
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, clamped);
            sr.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        Object.Destroy(obj);
    }

    private static Sprite LoadAttackSprite(string resourceName)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
            return null;

        if (resourceSpriteCache.TryGetValue(resourceName, out Sprite cached))
            return cached;

        string path = resourceName.StartsWith("AttackEffect/")
            ? resourceName
            : $"AttackEffect/{resourceName}";

        Texture2D texture = Resources.Load<Texture2D>(path);
        if (texture == null)
        {
            Debug.LogWarning($"[PlayerAttackVFX] Missing Resources/{path}.");
            return null;
        }

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            Vector2.one * 0.5f,
            PixelsPerUnit);

        resourceSpriteCache[resourceName] = sprite;
        return sprite;
    }

    private static void PlayCameraShake(HandRankType rank)
    {
        float duration = GetShakeDuration(rank);
        float magnitude = GetShakeMagnitude(rank);

        if (duration > 0f && magnitude > 0f)
            CameraShake.ShakeAll(duration, magnitude);
    }

    private static float GetShakeDuration(HandRankType rank)
    {
        switch (rank)
        {
            case HandRankType.OnePair: return 0.14f;
            case HandRankType.TwoPair: return 0.16f;
            case HandRankType.ThreeOfAKind: return 0.18f;
            case HandRankType.Straight: return 0.2f;
            case HandRankType.Flush: return 0.22f;
            case HandRankType.FullHouse: return 0.25f;
            case HandRankType.FourOfAKind: return 0.28f;
            case HandRankType.StraightFlush: return 0.32f;
            case HandRankType.RoyalFlush: return 0.38f;
            case HandRankType.HighCard:
            default: return 0f;
        }
    }

    private static float GetShakeMagnitude(HandRankType rank)
    {
        switch (rank)
        {
            case HandRankType.OnePair: return 0.09f;
            case HandRankType.TwoPair: return 0.125f;
            case HandRankType.ThreeOfAKind: return 0.16f;
            case HandRankType.Straight: return 0.195f;
            case HandRankType.Flush: return 0.24f;
            case HandRankType.FullHouse: return 0.3f;
            case HandRankType.FourOfAKind: return 0.37f;
            case HandRankType.StraightFlush: return 0.45f;
            case HandRankType.RoyalFlush: return 0.55f;
            case HandRankType.HighCard:
            default: return 0f;
        }
    }

    private static HandRankVFXSetting GetSetting(HandRankType rank)
    {
        foreach (HandRankVFXSetting setting in rankSettings)
            if (setting != null && setting.rank == rank)
                return setting;

        return null;
    }

    private static HandRankVFXSetting[] CreateDefaultSettings()
    {
        return new[]
        {
            CreateSetting(HandRankType.HighCard, "BasicSlash", Color.white, 2.52f, 0.14f, Vector2.zero),
            CreateSetting(HandRankType.OnePair, "BasicSlash", new Color(1f, 0.95f, 0.55f, 1f), 2.73f, 0.16f, Vector2.zero),
            CreateSetting(HandRankType.TwoPair, "CrossSlash", new Color(0.85f, 0.5f, 1f, 1f), 2.94f, 0.18f, Vector2.zero),
            CreateSetting(HandRankType.ThreeOfAKind, "FourImpact", new Color(1f, 0.62f, 0.25f, 1f), 2.84f, 0.2f, Vector2.zero),
            CreateSetting(HandRankType.Straight, "CardWave", new Color(0.35f, 1f, 0.85f, 1f), 3.47f, 0.2f, new Vector2(-2.4f, 0f)),
            CreateSetting(HandRankType.Flush, "FlushRing", new Color(0.55f, 0.7f, 1f, 1f), 3.05f, 0.22f, Vector2.zero),
            CreateSetting(HandRankType.FullHouse, new[] { "FlushRing", "FourImpact" }, null, new Color(1f, 0.55f, 0.85f, 1f), 3.15f, 0.24f, Vector2.zero),
            CreateSetting(HandRankType.FourOfAKind, "FourImpact", new Color(1f, 0.38f, 0.22f, 1f), 3.36f, 0.26f, Vector2.zero),
            CreateSetting(HandRankType.StraightFlush, new[] { "CardWave", "FlushRing" }, new[] { new Vector2(-2.1f, 0f), Vector2.zero }, new Color(0.3f, 1f, 0.92f, 1f), 3.68f, 0.28f, Vector2.zero),
            CreateSetting(HandRankType.RoyalFlush, "RoyalBurst", new Color(1f, 0.86f, 0.2f, 1f), 3.78f, 0.34f, Vector2.zero)
        };
    }

    private static HandRankVFXSetting CreateSetting(HandRankType rank, string resourceName, Color color, float scale, float duration, Vector2 offset)
    {
        return CreateSetting(rank, new[] { resourceName }, null, color, scale, duration, offset);
    }

    private static HandRankVFXSetting CreateSetting(HandRankType rank, string[] resourceNames, Vector2[] resourceOffsets, Color color, float scale, float duration, Vector2 offset)
    {
        return new HandRankVFXSetting
        {
            rank = rank,
            resourceSpriteNames = resourceNames,
            resourceOffsets = resourceOffsets,
            color = color,
            scale = scale,
            duration = duration,
            worldOffset = offset
        };
    }
}

public class PlayerAttackVFXRunner : MonoBehaviour
{
}
