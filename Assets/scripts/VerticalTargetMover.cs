using UnityEngine;

// Archery Target 전용 이동 스크립트.
// 처음 놓인 X/Z 위치는 유지하고 Y축으로만 위아래 이동한다.
[RequireComponent(typeof(TargetDestroy))]
public class VerticalTargetMover : MonoBehaviour
{
    public float moveHeight = 2.1f;
    public float moveSpeed = 1.2f;

    TargetDestroy targetDestroy;
    Vector3 basePosition;
    float phaseOffset;

    void Start()
    {
        targetDestroy = GetComponent<TargetDestroy>();
        basePosition = transform.position;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // 피격되어 사라진 동안에는 위치를 고정해서 리스폰이 흔들리지 않게 한다.
        if (targetDestroy != null && targetDestroy.IsRespawning)
        {
            return;
        }

        float lift = (Mathf.Sin(Time.time * moveSpeed + phaseOffset) + 1f) * 0.5f * moveHeight;
        transform.position = basePosition + Vector3.up * lift;
    }
}
