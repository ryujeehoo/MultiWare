using UnityEngine;
using UnityEngine.UI;

public class BattleBackgroundManager : MonoBehaviour
{
    public static BattleBackgroundManager Instance { get; private set; }

    [Header("Target")]
    public Image backgroundImage;
    public SpriteRenderer backgroundRenderer;

    [Header("Default Backgrounds")]
    public Sprite fungalCaveBackground;
    public Sprite crystalCaveBackground;

    private const string FungalCaveResource = "Battle_Background_1";
    private const string CrystalCaveResource = "Battle_Background_2";

    public static BattleBackgroundManager GetOrCreate()
    {
        Image sceneBackgroundImage = FindSceneBackgroundImage();

        if (Instance != null)
        {
            if (sceneBackgroundImage != null)
                Instance.backgroundImage = sceneBackgroundImage;
            Instance.LoadDefaultResources();
            return Instance;
        }

        BattleBackgroundManager existing = FindFirstObjectByType<BattleBackgroundManager>();
        if (existing != null)
        {
            if (sceneBackgroundImage != null)
                existing.backgroundImage = sceneBackgroundImage;
            existing.LoadDefaultResources();
            return existing;
        }

        if (sceneBackgroundImage != null)
        {
            BattleBackgroundManager sceneManager = sceneBackgroundImage.GetComponent<BattleBackgroundManager>();
            if (sceneManager == null)
                sceneManager = sceneBackgroundImage.gameObject.AddComponent<BattleBackgroundManager>();

            sceneManager.backgroundImage = sceneBackgroundImage;
            sceneManager.LoadDefaultResources();
            return sceneManager;
        }

        Debug.LogWarning("BattleBackgroundManager: Scene Background Image를 찾지 못했습니다. Hierarchy의 Background 오브젝트에 Image 컴포넌트가 있는지 확인하세요.");
        return null;
    }

    void Awake()
    {
        Instance = this;
        if (backgroundImage == null)
            backgroundImage = FindSceneBackgroundImage();
        LoadDefaultResources();
    }

    public void ApplyEncounter(MonsterEncounter encounter)
    {
        if (encounter == null) return;

        Sprite selected = encounter.backgroundSprite;
        if (selected == null)
            selected = SelectByMonsters(encounter.monsterPrefabSlots);

        ApplyBackground(selected);
    }

    public void ApplyBackground(Sprite background)
    {
        if (background == null) return;

        if (backgroundImage == null)
            backgroundImage = FindSceneBackgroundImage();

        if (backgroundImage != null)
            backgroundImage.sprite = background;

        if (backgroundRenderer != null)
            backgroundRenderer.sprite = background;
    }

    private Sprite SelectByMonsters(GameObject[] monsterPrefabs)
    {
        if (monsterPrefabs == null)
            return fungalCaveBackground;

        foreach (GameObject prefab in monsterPrefabs)
        {
            if (prefab == null) continue;

            string monsterName = prefab.name;
            Monster monster = prefab.GetComponent<Monster>();
            if (monster != null && monster.Data != null && !string.IsNullOrEmpty(monster.Data.monsterName))
                monsterName += " " + monster.Data.monsterName;

            if (IsCrystalMonster(monsterName))
                return crystalCaveBackground;
        }

        return fungalCaveBackground;
    }

    private bool IsCrystalMonster(string monsterName)
    {
        if (string.IsNullOrEmpty(monsterName)) return false;

        return monsterName.Contains("Monolit")
            || monsterName.Contains("Monolith")
            || monsterName.Contains("모노")
            || monsterName.Contains("전기")
            || monsterName.Contains("Electric")
            || monsterName.Contains("Sphere")
            || monsterName.Contains("Bug 2")
            || monsterName.Contains("Bug2");
    }

    private void LoadDefaultResources()
    {
        if (fungalCaveBackground == null)
            fungalCaveBackground = Resources.Load<Sprite>(FungalCaveResource);

        if (crystalCaveBackground == null)
            crystalCaveBackground = Resources.Load<Sprite>(CrystalCaveResource);
    }

    private static Image FindSceneBackgroundImage()
    {
        Image exact = null;
        Image fallback = null;

        Image[] images = FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (Image image in images)
        {
            if (image == null) continue;
            if (IsGeneratedBackground(image.transform)) continue;

            string objectName = image.gameObject.name.ToLowerInvariant();
            string parentName = image.transform.parent != null ? image.transform.parent.name.ToLowerInvariant() : "";

            if (objectName == "background" && parentName == "background")
                return image;

            if (exact == null && objectName == "background")
                exact = image;

            if (fallback == null && (objectName.Contains("background") || parentName.Contains("background")))
                fallback = image;
        }

        return exact != null ? exact : fallback;
    }

    private static bool IsGeneratedBackground(Transform target)
    {
        while (target != null)
        {
            string objectName = target.name.ToLowerInvariant();
            if (objectName.Contains("battlebackground canvas") || objectName.Contains("battle background"))
                return true;

            target = target.parent;
        }

        return false;
    }
}