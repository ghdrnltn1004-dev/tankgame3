using UnityEngine;

// TankGame5에서 가져온 단순 이동 예제용 스크립트.
// 현재 SampleScene의 주 탱크는 TankMove.cs(NewMonoBehaviourScript)를 사용한다.
public class TankMovePort : MonoBehaviour
{
    // 전진/후진 속도와 좌우 회전 속도
    float moveSpeed = 10f;
    float rotateSpeed = 60f;

    void Update()
    {
        float amount = moveSpeed * Time.deltaTime;
        float amountRotate = rotateSpeed * Time.deltaTime;

        // Unity 기본 입력축: W/S 또는 위/아래, A/D 또는 왼쪽/오른쪽
        float vert = Input.GetAxis("Vertical");
        float horz = Input.GetAxis("Horizontal");

        // 탱크가 바라보는 방향으로 전후 이동하고, Y축 기준으로 회전한다.
        transform.Translate(Vector3.forward * amount * vert);
        transform.Rotate(Vector3.up * amountRotate * horz);
    }
}
