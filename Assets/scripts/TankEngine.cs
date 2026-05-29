using UnityEngine;

// 탱크가 움직이거나 회전할 때 엔진 소리의 높이/볼륨을 바꾸는 스크립트.
public class TankEngine : MonoBehaviour
{
    AudioSource soundEngine;

    void Start()
    {
        soundEngine = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (soundEngine == null)
        {
            return;
        }

        // 앞뒤/좌우 입력 중 더 큰 값을 기준으로 엔진 세기를 정한다.
        float vert = Mathf.Abs(Input.GetAxis("Vertical"));
        float horz = Mathf.Abs(Input.GetAxis("Horizontal"));
        float pitch = Mathf.Max(vert, horz);

        // 멈춰 있을 때는 기본 톤, 움직일수록 더 높은 톤과 볼륨으로 들리게 한다.
        soundEngine.pitch = pitch + 1f;
        soundEngine.volume = soundEngine.pitch * 0.6f;
    }
}
