using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// SampleScene에 EnvLevel 패키지의 환경 맵을 자동으로 배치한다.
// 기본값은 사막 맵이며, 깨져 보이는 회색 바닥을 덮는 게임용 바닥도 같이 만든다.
[ExecuteAlways]
public class SampleEnvironmentLevelBuilder : MonoBehaviour
{
    public enum EnvironmentLevel
    {
        Desert,
        Jungle,
        Moon
    }

    const string GeneratedLevelName = "Generated EnvLevel Map";
    const string SolidFloorName = "Stable Desert Floor";
    const string SimpleArenaName = "Sample Battle Arena";

    const string DesertPath = "Assets/_Tanks/Prefabs/Levels/LevelDesert.prefab";
    const string JunglePath = "Assets/_Tanks/Prefabs/Levels/LevelJungle.prefab";
    const string MoonPath = "Assets/_Tanks/Prefabs/Levels/LevelMoon.prefab";

    public EnvironmentLevel activeLevel = EnvironmentLevel.Desert;
    public GameObject desertLevelPrefab;
    public GameObject jungleLevelPrefab;
    public GameObject moonLevelPrefab;

    // 사막 바닥 재질. 비어 있으면 실행 중에 단색 모래 재질을 만들어 쓴다.
    public Material desertFloorMaterial;

    // 기존 탱크/타겟 높이에 맞춰 맵 바닥을 조금 아래로 내려둔다.
    public Vector3 levelPosition = new Vector3(0f, -0.92f, 4f);
    public Vector3 levelRotation = Vector3.zero;
    public Vector3 levelScale = Vector3.one;

    // EnvLevel 원본 바닥의 얇은 조각 사이로 보이는 회색 틈을 덮기 위한 안정 바닥 설정
    public bool addStableFloor = true;
    public Vector2 stableFloorSize = new Vector2(118f, 118f);
    public float stableFloorYOffset = 0.035f;

    void OnEnable()
    {
        if (!Application.isPlaying)
        {
            BuildLevel(false);
        }
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            BuildLevel(true);
        }
    }

    void BuildLevel(bool rebuildExisting)
    {
        if (!gameObject.scene.IsValid() || gameObject.scene.name != "SampleScene")
        {
            return;
        }

        HideSimpleArena();

        GameObject existingLevel = GameObject.Find(GeneratedLevelName);
        if (existingLevel != null)
        {
            if (!rebuildExisting)
            {
                EnsureStableFloor(existingLevel.transform);
                return;
            }

            existingLevel.name = GeneratedLevelName + " Old";
            existingLevel.SetActive(false);
            Destroy(existingLevel);
        }

        GameObject prefab = GetSelectedPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("EnvLevel 맵 프리팹을 찾지 못했습니다.");
            return;
        }

        GameObject level = InstantiateLevelPrefab(prefab);
        level.name = GeneratedLevelName;
        level.transform.position = levelPosition;
        level.transform.rotation = Quaternion.Euler(levelRotation);
        level.transform.localScale = levelScale;

        EnsureStableFloor(level.transform);

        if (!Application.isPlaying)
        {
            SetDontSaveInEditor(level);
        }
    }

    GameObject GetSelectedPrefab()
    {
        switch (activeLevel)
        {
            case EnvironmentLevel.Jungle:
                return LoadLevelPrefab(jungleLevelPrefab, JunglePath);
            case EnvironmentLevel.Moon:
                return LoadLevelPrefab(moonLevelPrefab, MoonPath);
            default:
                return LoadLevelPrefab(desertLevelPrefab, DesertPath);
        }
    }

    void EnsureStableFloor(Transform level)
    {
        if (!addStableFloor || activeLevel != EnvironmentLevel.Desert || level == null)
        {
            return;
        }

        Transform existingFloor = level.Find(SolidFloorName);
        if (existingFloor != null)
        {
            existingFloor.localPosition = new Vector3(0f, stableFloorYOffset, 0f);
            existingFloor.localScale = Vector3.one;
            return;
        }

        GameObject floor = new GameObject(SolidFloorName);
        floor.transform.SetParent(level, false);
        floor.transform.localPosition = new Vector3(0f, stableFloorYOffset, 0f);
        floor.transform.localRotation = Quaternion.identity;
        floor.transform.localScale = Vector3.one;

        MeshFilter meshFilter = floor.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = CreateFloorMesh(stableFloorSize);

        MeshRenderer meshRenderer = floor.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = ResolveFloorMaterial();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = true;

        if (!Application.isPlaying)
        {
            SetDontSaveInEditor(floor);
        }
    }

    Mesh CreateFloorMesh(Vector2 size)
    {
        float halfX = size.x * 0.5f;
        float halfZ = size.y * 0.5f;
        float uvRepeat = Mathf.Max(1f, size.x / 12f);

        Mesh mesh = new Mesh();
        mesh.name = "Stable Desert Floor Mesh";
        mesh.vertices = new[]
        {
            new Vector3(-halfX, 0f, -halfZ),
            new Vector3(halfX, 0f, -halfZ),
            new Vector3(-halfX, 0f, halfZ),
            new Vector3(halfX, 0f, halfZ)
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(uvRepeat, 0f),
            new Vector2(0f, uvRepeat),
            new Vector2(uvRepeat, uvRepeat)
        };
        mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.hideFlags = HideFlags.DontSave;
        return mesh;
    }

    Material ResolveFloorMaterial()
    {
        if (desertFloorMaterial != null)
        {
            return desertFloorMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        Color sandColor = new Color(0.93f, 0.72f, 0.3f);
        material.name = "Runtime Stable Sand";
        material.color = sandColor;
        material.hideFlags = HideFlags.DontSave;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", sandColor);
        }

        return material;
    }

    GameObject InstantiateLevelPrefab(GameObject prefab)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab, gameObject.scene);
        }
#endif

        return Instantiate(prefab);
    }

    GameObject LoadLevelPrefab(GameObject serializedPrefab, string assetPath)
    {
        if (serializedPrefab != null)
        {
            return serializedPrefab;
        }

#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
#else
        return null;
#endif
    }

    void HideSimpleArena()
    {
        GameObject simpleArena = GameObject.Find(SimpleArenaName);
        if (simpleArena != null)
        {
            simpleArena.SetActive(false);
        }
    }

    static void SetDontSaveInEditor(GameObject target)
    {
        Transform[] transforms = target.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in transforms)
        {
            child.gameObject.hideFlags = HideFlags.DontSaveInEditor;
        }
    }
}
