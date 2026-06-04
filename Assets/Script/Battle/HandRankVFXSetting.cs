using UnityEngine;

[System.Serializable]
public class HandRankVFXSetting
{
    public HandRankType rank;
    public string[] resourceSpriteNames;
    public Vector2[] resourceOffsets;
    public Sprite[] worldSprites;
    public GameObject prefab;
    public Color color = Color.white;
    public Vector2 worldOffset;
    public float scale = 1f;
    public float duration = 0.42f;
    public float prefabLifetime = 1.2f;
}
