using UnityEngine;

// TankGame5에서 가져온 카메라 추적 예제.
// 현재 SampleScene에서는 CameraFollow.cs가 주 카메라를 담당하지만, 이 스크립트도 참고용으로 남겨둔다.
public class FollowTarget : MonoBehaviour
{
    [Header("Camera Offset")]
    [SerializeField] Vector3 position = new Vector3(0f, 3.6f, -7.8f);
    [SerializeField] Vector3 rotation = new Vector3(14f, 0f, 0f);
    [SerializeField][Range(10f, 100f)] float fov = 30f;

    [Header("Camera Follow Speed")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float turnSpeed = 10f;

    Transform target;
    Transform cam;
    Transform pivot;
    Transform pivotRot;

    void Start()
    {
        target = GameObject.Find("Tank").transform;
        InitCamera();
    }

    void Update()
    {
        // 마우스 휠로 카메라 시야각을 조절한다.
        float zoom = Input.GetAxis("Mouse ScrollWheel") * 20f;
        fov = Mathf.Clamp(fov - zoom, 10f, 100f);
        cam.GetComponent<Camera>().fieldOfView = fov;

        // 오른쪽 마우스를 누르는 동안에만 카메라 회전을 받는다.
        if (!Input.GetMouseButton(1))
        {
            return;
        }

        float x = Input.GetAxis("Mouse Y") * 2f;
        float y = Input.GetAxis("Mouse X") * 2f;

        Vector3 ang = pivotRot.localEulerAngles + new Vector3(x, y, 0f);
        if (ang.x > 180f)
        {
            ang.x -= 360f;
        }

        // 카메라가 바닥 아래나 너무 위로 돌아가지 않게 제한한다.
        ang.x = Mathf.Clamp(ang.x, -24f, 80f);
        pivotRot.localEulerAngles = ang;
    }

    void FixedUpdate()
    {
        // Pivot은 탱크 위치/회전을 부드럽게 따라간다.
        Vector3 pos = target.position;
        Quaternion rot = target.rotation;

        pivot.position = Vector3.Lerp(pivot.position, pos, moveSpeed * Time.deltaTime);
        pivot.rotation = Quaternion.Lerp(pivot.rotation, rot, turnSpeed * Time.deltaTime);
    }

    void InitCamera()
    {
        cam = Camera.main.transform;
        cam.GetComponent<Camera>().fieldOfView = fov;

        // Pivot 구조를 만들어 카메라를 탱크 기준으로 움직이게 한다.
        pivot = new GameObject("Pivot").transform;
        pivot.position = target.position;

        pivotRot = new GameObject("PivotRot").transform;
        pivotRot.position = target.position;
        pivotRot.parent = pivot;

        cam.parent = pivotRot;
        cam.localPosition = position;
        cam.localEulerAngles = rotation;
        cam.localRotation = Quaternion.Euler(rotation);
    }
}
