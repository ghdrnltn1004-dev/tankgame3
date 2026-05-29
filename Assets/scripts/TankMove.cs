using UnityEngine;

// 차량 이동 및 바퀴 회전을 담당하는 스크립트
public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 10f;            // 기본 이동 속도
    public float turnSpeed = 100f;           // 회전 속도
    public float shiftBoostMultiplier = 2f;  // Shift 키를 눌렀을 때 2배속
    public float qBoostMultiplier = 1.5f;    // Q 키를 눌렀을 때 1.5배속

    [Header("Wheel Settings")]
    public Transform[] wheels;               // 바퀴 오브젝트들 배열
    public float wheelRotateSpeed = 500f;    // 바퀴 회전 속도

    void Update()
    {
        // 좌우 입력값 받기 (A/D 또는 ←/→)
        float hori = Input.GetAxis("Horizontal");

        // 앞뒤 입력값 받기 (W/S 또는 ↑/↓)
        float vert = Input.GetAxis("Vertical");

        // 기본 이동 속도를 현재 속도로 설정
        float currentSpeed = moveSpeed;

        // Shift 키를 누르면 즉시 2배속 적용
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = moveSpeed * shiftBoostMultiplier;
        }
        // Q 키를 누르면 즉시 1.5배속 적용
        else if (Input.GetKey(KeyCode.Q))
        {
            currentSpeed = moveSpeed * qBoostMultiplier;
        }

        // 실제 이동량 계산
        float moveAmount = currentSpeed * Time.deltaTime;

        // 실제 회전량 계산
        float turnAmount = turnSpeed * Time.deltaTime;

        // 앞뒤 이동 처리
        // vert가 1이면 전진, -1이면 후진
        transform.Translate(Vector3.forward * vert * moveAmount);

        // 회전 처리
        // 후진할 때는 좌우 조작이 반대로 느껴지므로 방향 반전
        if (vert < 0)
        {
            transform.Rotate(Vector3.up * -hori * turnAmount);
        }
        else
        {
            transform.Rotate(Vector3.up * hori * turnAmount);
        }

        // 바퀴 회전 처리
        if (wheels != null)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                // 바퀴 오브젝트가 비어있지 않을 때만 회전
                if (wheels[i] != null)
                {
                    // 전진하면 정방향, 후진하면 역방향 회전
                    wheels[i].Rotate(Vector3.right * vert * wheelRotateSpeed * Time.deltaTime);
                }
            }
        }

        // 스페이스바를 누르면 로그 출력
        if (Input.GetButtonDown("Jump"))
        {
            Debug.Log("[Jump] Space bar pressed");
        }
    }
}