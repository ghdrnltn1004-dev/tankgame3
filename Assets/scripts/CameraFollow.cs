using UnityEngine;

// 탱크를 따라다니는 3인칭 카메라.
// 마우스 휠로 줌 인/아웃하고, 마우스 오른쪽 버튼 드래그로 카메라 방향을 돌린다.
public class CameraFollow : MonoBehaviour
{
    // 따라갈 탱크 Transform
    public Transform target;

    // 기본 카메라 위치와 따라가는 부드러움
    public Vector3 offset = new Vector3(0f, 5f, -8f);
    public float smoothSpeed = 5f;

    // 마우스 회전/줌 설정
    public float mouseSensitivity = 3f;
    public float zoomSpeed = 8f;
    public float minDistance = 4f;
    public float maxDistance = 15f;
    public float minPitch = 12f;
    public float maxPitch = 70f;

    // 현재 카메라 회전 각도와 탱크와의 거리
    float yaw;
    float pitch;
    float distance;

    void Start()
    {
        // 시작할 때 offset을 기준으로 카메라 거리와 위아래 각도를 계산한다.
        Vector3 flatOffset = new Vector3(offset.x, 0f, offset.z);
        distance = Mathf.Clamp(flatOffset.magnitude, minDistance, maxDistance);
        pitch = Mathf.Clamp(Mathf.Atan2(offset.y, distance) * Mathf.Rad2Deg, minPitch, maxPitch);

        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // 휠 입력으로 탱크와 카메라 사이 거리를 조절한다.
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
        }

        if (Input.GetMouseButton(1))
        {
            // 오른쪽 마우스를 누르는 동안 직접 카메라를 회전한다.
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        }
        else
        {
            // 마우스를 놓으면 카메라가 다시 탱크 뒤쪽으로 천천히 돌아간다.
            yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, smoothSpeed * 0.35f * Time.deltaTime);
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 계산된 회전/거리로 목표 위치를 만들고 부드럽게 이동한다.
        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + cameraRotation * new Vector3(0f, 0f, -distance);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 카메라는 항상 탱크를 바라본다.
        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, smoothSpeed * Time.deltaTime);
    }
}
