using UnityEngine;

// 총알의 전진, 충돌, 피격 대상 전달을 담당한다.
// 타겟 프리팹의 자식 Collider에 맞아도 부모의 TargetDestroy를 찾아 처리한다.
public class Bullet : MonoBehaviour
{
    // 총알이 앞으로 날아가는 속도
    float speed = 30f;

    void Start()
    {
        // 너무 멀리 날아간 총알은 자동 제거해서 씬에 계속 쌓이지 않게 한다.
        Destroy(gameObject, 2f);
    }

    void Update()
    {
        // 총알 자신의 앞 방향 기준으로 계속 전진한다.
        float amount = speed * Time.deltaTime;
        transform.Translate(Vector3.forward * amount);
    }

    void OnTriggerEnter(Collider other)
    {
        // Archery Target / Dummy Target처럼 여러 자식으로 된 모델도 부모의 파괴 스크립트를 찾는다.
        TargetDestroy target = other.GetComponentInParent<TargetDestroy>();
        if (target != null)
        {
            target.DestroySelf(transform.position);
            Destroy(gameObject);
            return;
        }

        // 적 포탑은 자식 Collider에 맞을 수 있어서 루트 오브젝트로 파괴 메시지를 보낸다.
        if (other.CompareTag("Enemy"))
        {
            other.transform.root.SendMessage("DestroySelf", transform.position);
        }

        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
    }
}
