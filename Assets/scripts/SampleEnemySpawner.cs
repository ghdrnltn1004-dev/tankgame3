using System.Collections.Generic;
using UnityEngine;

// 터렛 적군 수를 일정하게 유지하는 자동 생성기.
// 시작 터렛이 파괴되면 맵 안쪽에 새 터렛을 보충해서 maxEnemies 수를 맞춘다.
public class SampleEnemySpawner : MonoBehaviour
{
    // 생성할 터렛, 터렛 총알, 터렛 파괴 이펙트
    public Transform enemyPrefab;
    public Transform bulletPrefab;
    public Transform explosionPrefab;

    // 동시에 존재할 수 있는 터렛 수와 생성 간격
    public int maxEnemies = 2;
    public float spawnInterval = 5f;

    // 생성 위치/크기 설정
    public float enemyScale = 1.5f;
    public float spawnY = -0.695f;
    public Vector2 arenaMin = new Vector2(-10.5f, -9.5f);
    public Vector2 arenaMax = new Vector2(10.5f, 19f);

    // 탱크 바로 옆에 적이 생기지 않도록 비워두는 거리
    public float tankSafeRadius = 5f;

    // 현재 살아있는 터렛 목록
    readonly List<Enemy> enemies = new List<Enemy>();
    Transform tank;
    float nextSpawnTime;

    void Start()
    {
        GameObject tankObject = GameObject.Find("Tank");
        if (tankObject != null)
        {
            tank = tankObject.transform;
        }

        // 장면에 이미 배치된 터렛도 목록에 포함한다.
        enemies.AddRange(FindObjectsByType<Enemy>(FindObjectsSortMode.None));
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        // 이미 파괴된 터렛은 목록에서 제거한다.
        enemies.RemoveAll(enemy => enemy == null);

        if (enemyPrefab == null || enemies.Count >= maxEnemies || Time.time < nextSpawnTime)
        {
            return;
        }

        SpawnEnemy();
        nextSpawnTime = Time.time + spawnInterval;
    }

    void SpawnEnemy()
    {
        // 안전한 위치를 고른 뒤 터렛을 생성한다.
        Vector3 position = PickSpawnPosition();
        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Transform enemyTransform = Instantiate(enemyPrefab, position, rotation);
        enemyTransform.localScale = Vector3.one * enemyScale;

        Enemy enemy = enemyTransform.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.bullet = bulletPrefab;
            enemy.explosion = explosionPrefab;
            enemies.Add(enemy);
        }
    }

    Vector3 PickSpawnPosition()
    {
        // 여러 번 후보 위치를 뽑아 탱크와 너무 가까운 곳은 피한다.
        for (int i = 0; i < 24; i++)
        {
            Vector3 candidate = RandomPosition();
            if (tank == null || Vector3.Distance(candidate, tank.position) >= tankSafeRadius)
            {
                return candidate;
            }
        }

        return RandomPosition();
    }

    Vector3 RandomPosition()
    {
        // 투기장 안쪽 범위에서 랜덤 좌표를 만든다.
        return new Vector3(
            Random.Range(arenaMin.x, arenaMax.x),
            spawnY,
            Random.Range(arenaMin.y, arenaMax.y));
    }
}
