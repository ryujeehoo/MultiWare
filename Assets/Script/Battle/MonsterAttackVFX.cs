using System.Collections;
using UnityEngine;

public class MonsterAttackVFX : MonoBehaviour
{
    [SerializeField] private int sortingOrder = 50;
    [SerializeField] private float projectileHeight = 0.45f;

    private static Sprite pixelSprite;

    public IEnumerator PlayAnticipation(MonsterAction action)
    {
        MonsterAttackVFXType type = ResolveType(action);
        Color color = ResolveColor(action, type);

        switch (type)
        {
            case MonsterAttackVFXType.HeavyAttack:
                yield return StartCoroutine(ScalePulse(transform, 0.14f, 1.12f));
                break;
            case MonsterAttackVFXType.Shield:
            case MonsterAttackVFXType.ShieldPulse:
            case MonsterAttackVFXType.Heal:
            case MonsterAttackVFXType.HealPulse:
                yield return StartCoroutine(Ring(transform.position, color, 0.16f, 0.45f, 1.35f));
                break;
            case MonsterAttackVFXType.Summon:
            case MonsterAttackVFXType.SummonPulse:
                yield return StartCoroutine(SplitPulse(transform.position, color));
                break;
            case MonsterAttackVFXType.Burrow:
            case MonsterAttackVFXType.BurrowStrike:
                yield return StartCoroutine(DustCloud(transform.position, color, 0.14f));
                break;
            default:
                yield return StartCoroutine(Flash(transform.position, color, 0.1f, 0.55f));
                break;
        }
    }

    public IEnumerator PlayAttack(MonsterAction action, Vector3 source, Vector3 target)
    {
        MonsterAttackVFXType type = ResolveType(action);
        Color color = ResolveColor(action, type);

        switch (type)
        {
            case MonsterAttackVFXType.BiteStrike:
            case MonsterAttackVFXType.Dash:
                yield return StartCoroutine(Slash(target, color));
                break;
            case MonsterAttackVFXType.HeavyAttack:
            case MonsterAttackVFXType.StoneCrush:
                yield return StartCoroutine(Crush(target, color));
                yield return StartCoroutine(BurstParticles(target, color, 12, 0.55f, 0.12f, 0.14f, 0.32f));
                break;
            case MonsterAttackVFXType.Charge:
                yield return StartCoroutine(TrailDash(source, target, color));
                StartCoroutine(BurstParticles(target, color, 10, 0.48f, 0.1f, 0.08f, 0.24f));
                yield return StartCoroutine(DoubleRing(target, color, 0.1f, 0.25f, 1.4f));
                break;
            case MonsterAttackVFXType.Poison:
            case MonsterAttackVFXType.PoisonSting:
                yield return StartCoroutine(Needle(source, target, color, 0.09f));
                break;
            case MonsterAttackVFXType.RootBind:
            case MonsterAttackVFXType.RootSlam:
                yield return StartCoroutine(GroundSpike(target, color));
                yield return StartCoroutine(RootWrap(target, color));
                yield return StartCoroutine(BurstParticles(target + Vector3.down * 0.2f, color, 8, 0.35f, 0.08f, 0.1f, 0.28f));
                break;
            case MonsterAttackVFXType.CrystalAttack:
                yield return StartCoroutine(CrystalBurst(source, target, color));
                break;
            case MonsterAttackVFXType.Burrow:
            case MonsterAttackVFXType.BurrowStrike:
                yield return StartCoroutine(Burrow(source, target, color));
                break;
            case MonsterAttackVFXType.SlimeSplash:
                yield return StartCoroutine(ArcProjectile(source, target, color, 0.14f, 0.35f));
                yield return StartCoroutine(Splash(target, color));
                break;
            case MonsterAttackVFXType.ElectricBolt:
                yield return StartCoroutine(Bolt(source, target, color, 0.1f));
                StartCoroutine(BurstParticles(target, color, 12, 0.55f, 0.08f, 0.08f, 0.22f));
                yield return StartCoroutine(DoubleRing(target, color, 0.1f, 0.35f, 1.35f));
                break;
            case MonsterAttackVFXType.SelfDestruct:
                StartCoroutine(BurstParticles(source, color, 18, 0.9f, 0.16f, 0.14f, 0.38f));
                yield return StartCoroutine(DoubleRing(source, color, 0.2f, 0.8f, 2.4f));
                yield return StartCoroutine(DoubleRing(target, color, 0.12f, 0.5f, 1.6f));
                break;
            case MonsterAttackVFXType.Auto:
            default:
                yield return StartCoroutine(Slash(target, color));
                break;
        }
    }

    public IEnumerator PlaySupport(MonsterAction action)
    {
        MonsterAttackVFXType type = ResolveType(action);
        Color color = ResolveColor(action, type);

        switch (type)
        {
            case MonsterAttackVFXType.Summon:
            case MonsterAttackVFXType.SummonPulse:
                yield return StartCoroutine(SplitPulse(transform.position, color));
                yield return StartCoroutine(BurstParticles(transform.position, color, 8, 0.45f, 0.1f, 0.08f, 0.24f));
                break;
            case MonsterAttackVFXType.Heal:
            case MonsterAttackVFXType.HealPulse:
                StartCoroutine(DoubleRing(transform.position, color, 0.18f, 0.35f, 1.3f));
                yield return StartCoroutine(Flash(transform.position, color, 0.35f, 0.7f));
                yield return StartCoroutine(RisingParticles(transform.position, color));
                break;
            case MonsterAttackVFXType.Shield:
            case MonsterAttackVFXType.ShieldPulse:
                yield return StartCoroutine(DoubleRing(transform.position, color, 0.22f, 0.6f, 1.65f));
                yield return StartCoroutine(ShieldSparkle(transform.position, color));
                break;
            default:
                yield return StartCoroutine(Ring(transform.position, color, 0.28f, 0.35f, 1.4f));
                break;
        }
    }

    private MonsterAttackVFXType ResolveType(MonsterAction action)
    {
        if (action == null) return MonsterAttackVFXType.Auto;
        if (action.attackVFXType != MonsterAttackVFXType.Auto) return action.attackVFXType;

        switch (action.actionType)
        {
            case ActionType.Defend:
            case ActionType.AllyDefend:
            case ActionType.Reflect:
                return MonsterAttackVFXType.Shield;
            case ActionType.Summon:
                return MonsterAttackVFXType.Summon;
            case ActionType.Heal:
                return MonsterAttackVFXType.Heal;
            case ActionType.Shock:
                return MonsterAttackVFXType.ElectricBolt;
            case ActionType.Poison:
                return MonsterAttackVFXType.Poison;
            case ActionType.SelfDestruct:
                return MonsterAttackVFXType.SelfDestruct;
            case ActionType.AttackAndEffect:
                return action.attackEffectType == AttackEffectType.Shock
                    ? MonsterAttackVFXType.ElectricBolt
                    : MonsterAttackVFXType.Poison;
            case ActionType.MultiAttack:
                return MonsterAttackVFXType.Charge;
            default:
                return MonsterAttackVFXType.BiteStrike;
        }
    }

    private Color ResolveColor(MonsterAction action, MonsterAttackVFXType type)
    {
        if (action != null && action.attackVFXColor != Color.white)
            return action.attackVFXColor;

        switch (type)
        {
            case MonsterAttackVFXType.BiteStrike: return new Color(1f, 0.9f, 0.55f, 1f);
            case MonsterAttackVFXType.HeavyAttack: return new Color(1f, 0.55f, 0.28f, 1f);
            case MonsterAttackVFXType.Charge: return new Color(1f, 0.85f, 0.45f, 1f);
            case MonsterAttackVFXType.Poison: return new Color(0.7f, 0.35f, 1f, 1f);
            case MonsterAttackVFXType.RootBind: return new Color(0.7f, 0.45f, 0.22f, 1f);
            case MonsterAttackVFXType.Shield: return new Color(0.35f, 0.75f, 1f, 1f);
            case MonsterAttackVFXType.Summon: return new Color(0.75f, 0.4f, 1f, 1f);
            case MonsterAttackVFXType.Heal: return new Color(0.45f, 1f, 0.6f, 1f);
            case MonsterAttackVFXType.CrystalAttack: return new Color(0.65f, 0.95f, 1f, 1f);
            case MonsterAttackVFXType.Burrow: return new Color(0.95f, 0.65f, 0.32f, 1f);
            case MonsterAttackVFXType.SlimeSplash: return new Color(0.35f, 1f, 0.55f, 1f);
            case MonsterAttackVFXType.RootSlam: return new Color(0.75f, 0.45f, 0.22f, 1f);
            case MonsterAttackVFXType.ElectricBolt: return new Color(0.35f, 0.95f, 1f, 1f);
            case MonsterAttackVFXType.PoisonSting: return new Color(0.7f, 0.35f, 1f, 1f);
            case MonsterAttackVFXType.StoneCrush: return new Color(0.85f, 0.8f, 0.65f, 1f);
            case MonsterAttackVFXType.BurrowStrike: return new Color(0.95f, 0.65f, 0.32f, 1f);
            case MonsterAttackVFXType.ShieldPulse: return new Color(0.35f, 0.75f, 1f, 1f);
            case MonsterAttackVFXType.SummonPulse: return new Color(0.75f, 0.4f, 1f, 1f);
            case MonsterAttackVFXType.HealPulse: return new Color(0.45f, 1f, 0.6f, 1f);
            case MonsterAttackVFXType.SelfDestruct: return new Color(1f, 0.35f, 0.18f, 1f);
            default: return new Color(1f, 0.9f, 0.55f, 1f);
        }
    }

    private IEnumerator ArcProjectile(Vector3 source, Vector3 target, Color color, float duration, float size)
    {
        GameObject obj = CreateQuad("ArcProjectile", source, color, size);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            Vector3 pos = Vector3.Lerp(source, target, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * projectileHeight;
            obj.transform.position = pos;
            obj.transform.Rotate(0f, 0f, 360f * Time.deltaTime);
            SetAlpha(obj, 1f - Mathf.Max(0f, t - 0.75f) * 4f);
            yield return null;
        }
        Destroy(obj);
    }

    private IEnumerator Needle(Vector3 source, Vector3 target, Color color, float duration)
    {
        GameObject obj = CreateQuad("Needle", source, color, 0.18f);
        Vector3 dir = target - source;
        obj.transform.right = dir.normalized;
        obj.transform.localScale = new Vector3(0.85f, 0.08f, 1f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            obj.transform.position = Vector3.Lerp(source, target, t);
            yield return null;
        }
        Destroy(obj);
    }

    private IEnumerator Bolt(Vector3 source, Vector3 target, Color color, float duration)
    {
        int segments = 5;
        GameObject[] pieces = new GameObject[segments];
        for (int i = 0; i < segments; i++)
        {
            Vector3 a = Vector3.Lerp(source, target, i / (float)segments);
            Vector3 b = Vector3.Lerp(source, target, (i + 1f) / segments);
            Vector3 mid = (a + b) * 0.5f + (Vector3)Random.insideUnitCircle * 0.12f;
            pieces[i] = CreateQuad("Bolt", mid, color, 1f);
            pieces[i].transform.right = (b - a).normalized;
            pieces[i].transform.localScale = new Vector3(Vector3.Distance(a, b), 0.06f, 1f);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - elapsed / duration;
            for (int i = 0; i < pieces.Length; i++)
                SetAlpha(pieces[i], alpha);
            yield return null;
        }

        for (int i = 0; i < pieces.Length; i++)
            Destroy(pieces[i]);
    }

    private IEnumerator Slash(Vector3 target, Color color)
    {
        GameObject slash = CreateQuad("Slash", target, color, 1f);
        slash.transform.rotation = Quaternion.Euler(0f, 0f, -28f);
        slash.transform.localScale = new Vector3(1.1f, 0.12f, 1f);
        yield return StartCoroutine(FadeScale(slash, 0.09f, 0.8f, 1.35f));
    }

    private IEnumerator TrailDash(Vector3 source, Vector3 target, Color color)
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 pos = Vector3.Lerp(source, target, i / 8f);
            GameObject trail = CreateQuad("DashTrail", pos, color, Random.Range(0.25f, 0.48f));
            SetAlpha(trail, 0.58f - i * 0.055f);
            StartCoroutine(FadeScale(trail, 0.12f, 0.5f, 1.8f));
            yield return new WaitForSeconds(0.01f);
        }
    }

    private IEnumerator Splash(Vector3 target, Color color)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 0.45f;
            GameObject drop = CreateQuad("Splash", target + (Vector3)offset, color, Random.Range(0.12f, 0.25f));
            StartCoroutine(FadeScale(drop, 0.12f, 0.65f, 1.4f));
        }
        yield return new WaitForSeconds(0.12f);
    }

    private IEnumerator GroundSpike(Vector3 target, Color color)
    {
        for (int i = -2; i <= 2; i++)
        {
            GameObject spike = CreateQuad("RootSpike", target + new Vector3(i * 0.2f, -0.25f, 0f), color, 0.25f);
            spike.transform.localScale = new Vector3(0.12f, Random.Range(0.65f, 1.05f), 1f);
            yield return StartCoroutine(FadeScale(spike, 0.06f, 0.55f, 1.1f));
        }
        yield return StartCoroutine(Ring(target, color, 0.09f, 0.25f, 1.2f));
    }

    private IEnumerator RootWrap(Vector3 target, Color color)
    {
        for (int i = 0; i < 7; i++)
        {
            float angle = -65f + i * 22f;
            GameObject vine = CreateQuad("RootBind", target + new Vector3((i - 3f) * 0.08f, -0.08f, 0f), color, 1f);
            vine.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            vine.transform.localScale = new Vector3(0.06f, Random.Range(0.55f, 0.9f), 1f);
            StartCoroutine(FadeScale(vine, 0.18f, 0.85f, 1.12f));
        }

        yield return new WaitForSeconds(0.16f);
    }

    private IEnumerator Crush(Vector3 target, Color color)
    {
        GameObject block = CreateQuad("Crush", target + Vector3.up * 0.65f, color, 0.75f);
        float t = 0f;
        Vector3 start = block.transform.position;
        Vector3 end = target + Vector3.up * 0.05f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.09f;
            block.transform.position = Vector3.Lerp(start, end, t * t);
            yield return null;
        }
        Destroy(block);
        StartCoroutine(BurstParticles(target, color, 10, 0.5f, 0.08f, 0.1f, 0.28f));
        yield return StartCoroutine(DoubleRing(target, color, 0.1f, 0.35f, 1.55f));
    }

    private IEnumerator CrystalBurst(Vector3 source, Vector3 target, Color color)
    {
        const int count = 9;
        GameObject[] shards = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            float angle = i * (360f / count);
            Vector3 offset = Quaternion.Euler(0f, 0f, angle) * Vector3.right * Random.Range(0.25f, 0.48f);
            shards[i] = CreateQuad("CrystalShard", source + offset, color, Random.Range(0.16f, 0.26f));
            shards[i].transform.rotation = Quaternion.Euler(0f, 0f, angle);
            shards[i].transform.localScale = new Vector3(0.08f, Random.Range(0.34f, 0.55f), 1f);
        }

        float spinTime = 0.12f;
        float elapsed = 0f;
        while (elapsed < spinTime)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < shards.Length; i++)
            {
                if (shards[i] == null) continue;
                shards[i].transform.RotateAround(source, Vector3.forward, 540f * Time.deltaTime);
                shards[i].transform.Rotate(0f, 0f, 720f * Time.deltaTime);
            }
            yield return null;
        }

        for (int i = 0; i < shards.Length; i++)
            StartCoroutine(FlyShard(shards[i], target + (Vector3)Random.insideUnitCircle * 0.25f, 0.1f));

        yield return new WaitForSeconds(0.1f);
        StartCoroutine(BurstParticles(target, color, 14, 0.6f, 0.09f, 0.08f, 0.25f));
        yield return StartCoroutine(DoubleRing(target, color, 0.16f, 0.35f, 1.55f));
    }

    private IEnumerator Burrow(Vector3 source, Vector3 target, Color color)
    {
        yield return StartCoroutine(DustCloud(source, color, 0.08f));
        yield return new WaitForSeconds(0.04f);
        yield return StartCoroutine(DustCloud(target, color, 0.09f));
        yield return StartCoroutine(GroundSpike(target, color));
    }

    private IEnumerator DustCloud(Vector3 center, Color color, float duration)
    {
        Color dustColor = Color.Lerp(color, new Color(0.55f, 0.35f, 0.18f, 1f), 0.55f);
        for (int i = 0; i < 11; i++)
        {
            GameObject dust = CreateQuad("Dust", center + (Vector3)Random.insideUnitCircle * 0.35f, dustColor, Random.Range(0.14f, 0.42f));
            StartCoroutine(FloatAndFade(dust, duration + Random.Range(0f, 0.08f), (Vector3)Random.insideUnitCircle * 0.35f));
        }
        yield return new WaitForSeconds(duration);
    }

    private IEnumerator SplitPulse(Vector3 center, Color color)
    {
        StartCoroutine(Flash(center, color, 0.08f, 0.65f));

        for (int i = -1; i <= 1; i += 2)
        {
            GameObject split = CreateQuad("SplitPulse", center + Vector3.right * i * 0.16f, color, 0.36f);
            StartCoroutine(FloatAndFade(split, 0.18f, Vector3.right * i * 0.62f));
        }

        yield return StartCoroutine(BurstParticles(center, color, 6, 0.35f, 0.08f, 0.08f, 0.22f));
    }

    private IEnumerator RisingParticles(Vector3 center, Color color)
    {
        for (int i = 0; i < 14; i++)
        {
            Vector3 start = center + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.3f, 0.25f), 0f);
            GameObject particle = CreateQuad("HealParticle", start, color, Random.Range(0.08f, 0.22f));
            StartCoroutine(FloatAndFade(particle, Random.Range(0.35f, 0.6f), Vector3.up * Random.Range(0.45f, 1f)));
            yield return new WaitForSeconds(0.018f);
        }
    }

    private IEnumerator ShieldSparkle(Vector3 center, Color color)
    {
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f;
            Vector3 offset = Quaternion.Euler(0f, 0f, angle) * Vector3.right * Random.Range(0.45f, 0.85f);
            GameObject sparkle = CreateQuad("ShieldSparkle", center + offset, color, Random.Range(0.1f, 0.22f));
            StartCoroutine(FadeScale(sparkle, 0.18f, 0.95f, 1.9f));
        }

        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator ScalePulse(Transform target, float duration, float peakScale)
    {
        Vector3 start = target.localScale;
        Vector3 peak = start * peakScale;
        float half = duration * 0.5f;

        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(start, peak, elapsed / half);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(peak, start, elapsed / half);
            yield return null;
        }

        target.localScale = start;
    }

    private IEnumerator FloatAndFade(GameObject obj, float duration, Vector3 offset)
    {
        if (obj == null) yield break;

        Vector3 start = obj.transform.position;
        Vector3 end = start + offset;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            obj.transform.position = Vector3.Lerp(start, end, t);
            SetAlpha(obj, 1f - t);
            yield return null;
        }

        Destroy(obj);
    }

    private IEnumerator FlyShard(GameObject shard, Vector3 target, float duration)
    {
        if (shard == null) yield break;

        Vector3 start = shard.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            shard.transform.position = Vector3.Lerp(start, target, t);
            shard.transform.Rotate(0f, 0f, 900f * Time.deltaTime);
            yield return null;
        }

        Destroy(shard);
    }

    private IEnumerator Ring(Vector3 center, Color color, float duration, float startScale, float endScale)
    {
        GameObject ring = CreateQuad("Ring", center, color, startScale);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(startScale, endScale, t);
            ring.transform.localScale = new Vector3(scale, scale, 1f);
            SetAlpha(ring, 1f - t);
            yield return null;
        }
        Destroy(ring);
    }

    private IEnumerator DoubleRing(Vector3 center, Color color, float duration, float startScale, float endScale)
    {
        StartCoroutine(Ring(center, color, duration, startScale, endScale));
        Color secondary = Color.Lerp(color, Color.white, 0.35f);
        yield return StartCoroutine(Ring(center, secondary, duration * 0.85f, startScale * 0.65f, endScale * 0.82f));
    }

    private IEnumerator BurstParticles(Vector3 center, Color color, int count, float radius, float minSize, float maxSize, float duration)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = i * (360f / count) + Random.Range(-12f, 12f);
            Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.right;
            Vector3 start = center + direction * Random.Range(0.03f, 0.14f);
            Vector3 offset = direction * Random.Range(radius * 0.45f, radius);
            Color particleColor = Color.Lerp(color, Color.white, Random.Range(0f, 0.3f));
            GameObject particle = CreateQuad("BurstParticle", start, particleColor, Random.Range(minSize, maxSize));
            StartCoroutine(FloatAndFade(particle, duration * Random.Range(0.75f, 1.15f), offset));
        }

        yield return new WaitForSeconds(duration * 0.65f);
    }

    private IEnumerator Flash(Vector3 center, Color color, float duration, float scale)
    {
        GameObject flash = CreateQuad("Flash", center, color, scale);
        yield return StartCoroutine(FadeScale(flash, duration, 1f, 1.25f));
    }

    private IEnumerator FadeScale(GameObject obj, float duration, float alpha, float scaleMultiplier)
    {
        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            obj.transform.localScale = startScale * Mathf.Lerp(1f, scaleMultiplier, t);
            SetAlpha(obj, Mathf.Lerp(alpha, 0f, t));
            yield return null;
        }
        Destroy(obj);
    }

    private GameObject CreateQuad(string name, Vector3 position, Color color, float size)
    {
        EnsureSprite();
        GameObject obj = new GameObject(name);
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * size;

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = pixelSprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return obj;
    }

    private static void EnsureSprite()
    {
        if (pixelSprite != null) return;

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
                float alpha = Mathf.Clamp01(1f - distance);
                alpha = Mathf.SmoothStep(0f, 1f, alpha);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        pixelSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static void SetAlpha(GameObject obj, float alpha)
    {
        if (obj == null) return;
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer == null) return;

        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }
}
