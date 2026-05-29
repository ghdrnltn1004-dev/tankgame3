using UnityEngine;

// Target 장애물을 투기장 안에서 랜덤하게 돌아다니게 만든다.
// TargetDestroy가 리스폰 중일 때는 이동하지 않고, 다시 나타나면 이동을 이어간다.
[RequireComponent(typeof(Collider))]
public class RoamingTarget : MonoBehaviour
{
    // 이동/회전 속도와 이동 가능한 맵 범위
    public float moveSpeed = 1.2f;
    public float turnSpeed = 160f;
    public Vector2 arenaMin = new Vector2(-11.5f, -12.5f);
    public Vector2 arenaMax = new Vector2(11.5f, 20.5f);

    // 목적지에 도착했다고 판단하는 거리와 다음 목적지를 고르는 시간 범위
    public float destinationRadius = 0.35f;
    public Vector2 retargetTimeRange = new Vector2(4f, 8f);

    // 이동 중에 필요한 내부 상태
    TargetDestroy targetDestroy;
    Vector3 destination;
    float groundY;
    float nextRetargetTime;

    void Start()
    {
        targetDestroy = GetComponent<TargetDestroy>();
        groundY = transform.position.y;
        ClampInsideArena();
        PickDestination(Random.Range(0f, 1f));
    }

    void Update()
    {
        // 장애물이 맞아서 사라진 동안에는 움직이지 않는다.
        if (targetDestroy != null && targetDestroy.IsRespawning)
        {
            return;
        }

        // 목적지에 도착했거나 일정 시간이 지나면 새 목적지를 고른다.
        if (Time.time >= nextRetargetTime || ReachedDestination())
        {
            PickDestination(0f);
        }

        MoveToDestination();
    }

    void PickDestination(float extraDelay)
    {
        // 투기장 안쪽 랜덤 지점을 다음 이동 목표로 정한다.
        destination = new Vector3(
            Random.Range(arenaMin.x, arenaMax.x),
            groundY,
            Random.Range(arenaMin.y, arenaMax.y));

        float minimumTime = Mathf.Min(retargetTimeRange.x, retargetTimeRange.y);
        float maximumTime = Mathf.Max(retargetTimeRange.x, retargetTimeRange.y);
        nextRetargetTime = Time.time + extraDelay + Random.Range(minimumTime, maximumTime);
    }

    bool ReachedDestination()
    {
        Vector3 offset = destination - transform.position;
        offset.y = 0f;
        return offset.sqrMagnitude <= destinationRadius * destinationRadius;
    }

    void MoveToDestination()
    {
        Vector3 position = transform.position;
        Vector3 flatDirection = destination - position;
        flatDirection.y = 0f;

        if (flatDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        // 이동 방향을 바라보도록 회전한 뒤 목적지로 조금씩 이동한다.
        Quaternion targetRotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        Vector3 nextPosition = Vector3.MoveTowards(position, destination, moveSpeed * Time.deltaTime);
        nextPosition.x = Mathf.Clamp(nextPosition.x, arenaMin.x, arenaMax.x);
        nextPosition.y = groundY;
        nextPosition.z = Mathf.Clamp(nextPosition.z, arenaMin.y, arenaMax.y);
        transform.position = nextPosition;
    }

    void ClampInsideArena()
    {
        // 시작 위치가 범위 밖이면 강제로 맵 안쪽으로 넣는다.
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, arenaMin.x, arenaMax.x);
        position.y = groundY;
        position.z = Mathf.Clamp(position.z, arenaMin.y, arenaMax.y);
        transform.position = position;
    }
}
