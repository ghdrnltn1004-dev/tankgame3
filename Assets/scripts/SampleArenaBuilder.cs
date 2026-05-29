using UnityEngine;

// SampleScene에 전용 투기장 맵을 자동으로 만드는 스크립트.
// 직접 프리팹을 배치하지 않고 실행/편집 시점에 바닥, 벽, 기둥, 경계선을 생성한다.
[ExecuteAlways]
public class SampleArenaBuilder : MonoBehaviour
{
    const string ArenaName = "Sample Battle Arena";
    const float GroundY = -0.92f;

    void OnEnable()
    {
        // SampleScene에서만 만들고, 이미 만들어진 투기장이 있으면 중복 생성하지 않는다.
        if (!gameObject.scene.IsValid() || gameObject.scene.name != "SampleScene" || GameObject.Find(ArenaName) != null)
        {
            return;
        }

        Transform arena = new GameObject(ArenaName).transform;
        arena.position = Vector3.zero;

        // 편집 화면에서 자동 생성된 맵이 씬 파일에 저장되지 않도록 한다.
        if (!Application.isPlaying)
        {
            arena.gameObject.hideFlags = HideFlags.DontSaveInEditor;
        }

        Material floorMaterial = CreateMaterial("Arena Floor", new Color(0.23f, 0.27f, 0.29f));
        Material wallMaterial = CreateMaterial("Arena Wall", new Color(0.42f, 0.45f, 0.47f));
        Material postMaterial = CreateMaterial("Arena Post", new Color(0.13f, 0.19f, 0.23f));
        Material trimMaterial = CreateMaterial("Arena Trim", new Color(0.92f, 0.7f, 0.16f));

        // 투기장의 중심, 크기, 벽 높이/두께
        Vector3 center = new Vector3(0f, 0f, 4f);
        Vector2 size = new Vector2(28f, 38f);
        float wallHeight = 2.8f;
        float wallThickness = 0.8f;

        // 바닥 생성
        CreateBlock(arena, "Arena Floor", center + new Vector3(0f, GroundY - 0.095f, 0f),
            new Vector3(size.x, 0.2f, size.y), floorMaterial);

        // 사방 벽 생성
        CreateWall(arena, "North Wall", center, new Vector3(0f, 0f, size.y * 0.5f),
            new Vector3(size.x + wallThickness, wallHeight, wallThickness), wallMaterial);
        CreateWall(arena, "South Wall", center, new Vector3(0f, 0f, -size.y * 0.5f),
            new Vector3(size.x + wallThickness, wallHeight, wallThickness), wallMaterial);
        CreateWall(arena, "East Wall", center, new Vector3(size.x * 0.5f, 0f, 0f),
            new Vector3(wallThickness, wallHeight, size.y), wallMaterial);
        CreateWall(arena, "West Wall", center, new Vector3(-size.x * 0.5f, 0f, 0f),
            new Vector3(wallThickness, wallHeight, size.y), wallMaterial);

        CreateCornerPosts(arena, center, size, wallHeight + 0.45f, wallThickness, postMaterial);
        CreateBoundaryTrim(arena, center, size, wallThickness, trimMaterial);
    }

    static void CreateWall(Transform parent, string name, Vector3 center, Vector3 offset, Vector3 size, Material material)
    {
        Vector3 position = center + offset;
        position.y = GroundY + size.y * 0.5f;
        CreateBlock(parent, name, position, size, material);
    }

    static void CreateCornerPosts(Transform parent, Vector3 center, Vector2 size, float height, float wallThickness, Material material)
    {
        float x = size.x * 0.5f;
        float z = size.y * 0.5f;
        Vector3 postSize = new Vector3(wallThickness * 1.9f, height, wallThickness * 1.9f);

        // 네 모서리에 기둥을 세워 투기장 경계를 더 잘 보이게 한다.
        CreatePost(parent, "North East Post", center + new Vector3(x, 0f, z), postSize, material);
        CreatePost(parent, "North West Post", center + new Vector3(-x, 0f, z), postSize, material);
        CreatePost(parent, "South East Post", center + new Vector3(x, 0f, -z), postSize, material);
        CreatePost(parent, "South West Post", center + new Vector3(-x, 0f, -z), postSize, material);
    }

    static void CreatePost(Transform parent, string name, Vector3 position, Vector3 size, Material material)
    {
        position.y = GroundY + size.y * 0.5f;
        CreateBlock(parent, name, position, size, material);
    }

    static void CreateBoundaryTrim(Transform parent, Vector3 center, Vector2 size, float wallThickness, Material material)
    {
        // 바닥 위 노란 선으로 플레이 가능한 영역을 표시한다.
        float trimHeight = 0.035f;
        float trimWidth = 0.34f;
        float innerX = size.x * 0.5f - wallThickness - trimWidth;
        float innerZ = size.y * 0.5f - wallThickness - trimWidth;
        float y = GroundY + trimHeight * 0.5f + 0.012f;

        CreateBlock(parent, "North Trim", center + new Vector3(0f, y, innerZ),
            new Vector3(size.x - wallThickness * 2f, trimHeight, trimWidth), material, false);
        CreateBlock(parent, "South Trim", center + new Vector3(0f, y, -innerZ),
            new Vector3(size.x - wallThickness * 2f, trimHeight, trimWidth), material, false);
        CreateBlock(parent, "East Trim", center + new Vector3(innerX, y, 0f),
            new Vector3(trimWidth, trimHeight, size.y - wallThickness * 2f), material, false);
        CreateBlock(parent, "West Trim", center + new Vector3(-innerX, y, 0f),
            new Vector3(trimWidth, trimHeight, size.y - wallThickness * 2f), material, false);
    }

    static GameObject CreateBlock(Transform parent, string name, Vector3 position, Vector3 scale, Material material,
        bool colliderEnabled = true)
    {
        // 큐브 기본 도형을 크기 조절해서 바닥/벽/기둥/경계선으로 사용한다.
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(parent);
        block.transform.position = position;
        block.transform.localScale = scale;

        Renderer renderer = block.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        Collider collider = block.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = colliderEnabled;
        }

        return block;
    }

    static Material CreateMaterial(string name, Color color)
    {
        // URP 프로젝트면 URP/Lit을 쓰고, 없으면 기본 Standard 셰이더를 사용한다.
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = name;
        material.color = color;
        material.hideFlags = HideFlags.DontSave;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.1f);
        }

        return material;
    }
}
