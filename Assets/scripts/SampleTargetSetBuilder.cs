using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// SampleScene에서 기존 네모 타겟을 숨기고, 임포트된 두 종류의 타겟을 배치한다.
// Archery Target은 위아래 이동과 10점, Dummy Target FREE는 자유 이동과 20점을 가진다.
[ExecuteAlways]
public class SampleTargetSetBuilder : MonoBehaviour
{
    const string GeneratedRootName = "Generated Score Targets";
    const string ArcheryTargetPath = "Assets/Glowing Rifts/Archery Target/ArcheryTarget.prefab";
    const string DummyTargetPath = "Assets/Blink/Art/NPCs/Stylized/DummyTarget/DummyTarget.prefab";
    const float GroundY = -0.92f;

    public Transform explosionPrefab;
    public GameObject archeryPrefab;
    public GameObject dummyPrefab;

    readonly Vector3[] archeryPositions =
    {
        new Vector3(-4.2f, 0f, 8.5f),
        new Vector3(0f, 0f, 11.4f),
        new Vector3(4.2f, 0f, 14.3f)
    };

    readonly Vector3[] dummyPositions =
    {
        new Vector3(-2.1f, 0f, 10.1f),
        new Vector3(2.1f, 0f, 12.8f),
        new Vector3(0f, 0f, 16.2f)
    };

    void OnEnable()
    {
        if (!Application.isPlaying)
        {
            BuildTargets(false);
        }
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            BuildTargets(true);
        }
    }

    void BuildTargets(bool rebuildExisting)
    {
        if (!gameObject.scene.IsValid() || gameObject.scene.name != "SampleScene")
        {
            return;
        }

        HideLegacyCubeTargets();

        GameObject existingRoot = GameObject.Find(GeneratedRootName);
        if (existingRoot != null)
        {
            if (!rebuildExisting)
            {
                return;
            }

            // 플레이 시작 시에는 에디터에서 미리 만들어진 임시 타겟을 버리고 새로 만든다.
            existingRoot.name = GeneratedRootName + " Old";
            existingRoot.SetActive(false);
            Destroy(existingRoot);
        }

        GameObject loadedArcheryPrefab = LoadTargetPrefab(archeryPrefab, ArcheryTargetPath);
        GameObject loadedDummyPrefab = LoadTargetPrefab(dummyPrefab, DummyTargetPath);
        if (loadedArcheryPrefab == null || loadedDummyPrefab == null)
        {
            Debug.LogWarning("Archery Target 또는 Dummy Target FREE 프리팹을 찾지 못했습니다.");
            return;
        }

        GameObject root = new GameObject(GeneratedRootName);
        root.transform.position = Vector3.zero;

        if (!Application.isPlaying)
        {
            SetDontSaveInEditor(root);
        }

        for (int i = 0; i < archeryPositions.Length; i++)
        {
            CreateArcheryTarget(root.transform, loadedArcheryPrefab, i);
        }

        for (int i = 0; i < dummyPositions.Length; i++)
        {
            CreateDummyTarget(root.transform, loadedDummyPrefab, i);
        }
    }

    void HideLegacyCubeTargets()
    {
        TargetDestroy[] targets = FindObjectsByType<TargetDestroy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (TargetDestroy target in targets)
        {
            if (target != null && target.gameObject.name.StartsWith("Target"))
            {
                target.gameObject.SetActive(false);
            }
        }
    }

    void CreateArcheryTarget(Transform parent, GameObject prefab, int index)
    {
        GameObject target = CreateTarget(parent, prefab, "Archery Target " + (index + 1),
            archeryPositions[index], new Vector3(1.1f, 1.1f, 1.1f), 10);

        VerticalTargetMover mover = GetOrAddComponent<VerticalTargetMover>(target);
        mover.moveHeight = 1.25f;
        mover.moveSpeed = 1.35f + index * 0.15f;
    }

    void CreateDummyTarget(Transform parent, GameObject prefab, int index)
    {
        GameObject target = CreateTarget(parent, prefab, "Dummy Target FREE " + (index + 1),
            dummyPositions[index], new Vector3(1.35f, 1.35f, 1.35f), 20);

        RoamingTarget mover = GetOrAddComponent<RoamingTarget>(target);
        mover.moveSpeed = 2f;
        mover.turnSpeed = 180f;
        mover.arenaMin = new Vector2(-11.5f, -12.5f);
        mover.arenaMax = new Vector2(11.5f, 20.5f);
        mover.destinationRadius = 0.35f;
        mover.retargetTimeRange = new Vector2(2.5f, 5f);
    }

    GameObject CreateTarget(Transform parent, GameObject prefab, string targetName, Vector3 position, Vector3 scale, int score)
    {
        GameObject target = InstantiateTargetPrefab(prefab);
        target.name = targetName;
        target.tag = "Target";
        target.transform.SetParent(parent, true);
        target.transform.position = position;
        target.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        target.transform.localScale = scale;

        AlignBottomToGround(target);
        EnsureRootCollider(target);
        SetTargetTagOnColliders(target);

        TargetDestroy destroy = GetOrAddComponent<TargetDestroy>(target);
        destroy.explosion = explosionPrefab;
        destroy.respawnDelay = 3f;
        destroy.scoreValue = score;

        if (!Application.isPlaying)
        {
            SetDontSaveInEditor(target);
        }

        return target;
    }

    GameObject InstantiateTargetPrefab(GameObject prefab)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab, gameObject.scene);
        }
#endif

        return Instantiate(prefab);
    }

    GameObject LoadTargetPrefab(GameObject serializedPrefab, string assetPath)
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

    void AlignBottomToGround(GameObject target)
    {
        if (!TryGetRendererBounds(target, out Bounds bounds))
        {
            return;
        }

        Vector3 position = target.transform.position;
        position.y += GroundY - bounds.min.y;
        target.transform.position = position;
    }

    void EnsureRootCollider(GameObject target)
    {
        if (!TryGetRendererBounds(target, out Bounds bounds))
        {
            return;
        }

        BoxCollider box = target.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = target.AddComponent<BoxCollider>();
        }

        Vector3 scale = target.transform.lossyScale;
        box.center = target.transform.InverseTransformPoint(bounds.center);
        box.size = new Vector3(
            Mathf.Max(0.8f, bounds.size.x / Mathf.Max(0.01f, Mathf.Abs(scale.x))),
            Mathf.Max(0.8f, bounds.size.y / Mathf.Max(0.01f, Mathf.Abs(scale.y))),
            Mathf.Max(0.8f, bounds.size.z / Mathf.Max(0.01f, Mathf.Abs(scale.z))));
    }

    void SetTargetTagOnColliders(GameObject target)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        foreach (Collider targetCollider in colliders)
        {
            targetCollider.gameObject.tag = "Target";
        }
    }

    static bool TryGetRendererBounds(GameObject target, out Bounds bounds)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        bounds = new Bounds(target.transform.position, Vector3.one);

        if (renderers.Length == 0)
        {
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return true;
    }

    static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component == null)
        {
            component = target.AddComponent<T>();
        }

        return component;
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
