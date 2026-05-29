using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// TankGame5에서 가져온 이동+발사 통합 예제 스크립트.
// 현재 SampleScene의 주 탱크는 TankMove.cs와 SampleTankWeapon.cs를 나누어 사용하지만,
// 이 파일은 교재 예제 기능을 참고할 수 있도록 주석을 정리해서 남겨둔다.
public class TankMoveAndFire : MonoBehaviour
{
    // 이동/회전 기본값
    float moveSpeed = 10f;
    float rotateSpeed = 60f;

    // 총알이 생성될 위치와 총알 프리팹
    Transform spPoint;
    public Transform bullet;

    // 연사 제어용 값
    float delayTime = 0.1f;
    bool canFire = true;

    // 사운드, Rigidbody, 발사 화염, 체력, 폭발 프리팹
    AudioSource[] gunSound;
    Rigidbody rgBody;
    GameObject fire;
    int hp = 10;
    public Transform explosion;

    void Start()
    {
        // 씬 안의 SpawnPoint 오브젝트를 총알 발사 위치로 사용한다.
        spPoint = GameObject.Find("SpawnPoint").transform;

        // 단발/연사 사운드가 여러 AudioSource로 붙어 있다고 가정한다.
        gunSound = GetComponents<AudioSource>();
        rgBody = GetComponent<Rigidbody>();

        // 발사 화염은 쏠 때만 켜기 위해 시작 시 꺼둔다.
        fire = GameObject.Find("FireEffect");
        fire.SetActive(false);
    }

    void Update()
    {
        // 마우스 왼쪽 클릭: 단발 발사
        if (Input.GetButtonDown("Fire1"))
        {
            SingleShoot();
        }

        // LeftShift 누르는 동안: 연사 발사
        if (Input.GetKey(KeyCode.LeftShift) && canFire)
        {
            StartCoroutine(AutoFire2());
        }

        // 연사를 멈추면 연사 사운드도 멈춘다.
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            gunSound[1].Stop();
        }
    }

    void FixedUpdate()
    {
        // Rigidbody 이동은 FixedUpdate에서 처리한다.
        float vert = Input.GetAxis("Vertical");
        float horz = Input.GetAxis("Horizontal");

        float amountMove = moveSpeed * Time.fixedDeltaTime * vert;
        float amountRotate = rotateSpeed * Time.fixedDeltaTime * horz;

        rgBody.MovePosition(transform.position + transform.forward * amountMove);
        rgBody.MoveRotation(transform.rotation * Quaternion.Euler(Vector3.up * amountRotate));
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Tank hit: " + other.gameObject.name);

        // 총알에 맞을 때마다 체력을 줄인다.
        if (other.CompareTag("Bullet"))
        {
            hp--;
            Debug.Log("Tank hit! HP: " + hp);

            // 원본 예제 흐름을 유지하기 위해 hp가 0보다 작아지면 파괴 처리한다.
            if (hp < 0)
            {
                StartCoroutine(DestroySelf());
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Tank collided with: " + collision.gameObject.name);
    }

    void SingleShoot()
    {
        // 총알 생성, 단발 사운드 재생, 발사 화염 표시
        Instantiate(bullet, spPoint.position, spPoint.rotation);
        gunSound[0].Play();
        fire.SetActive(true);
    }

    void AutoFire()
    {
        // Update에서 직접 호출하는 방식의 연사 예제
        delayTime += Time.deltaTime;
        if (delayTime >= 0.1f)
        {
            delayTime = 0f;
            Instantiate(bullet, spPoint.position, spPoint.rotation);
        }
    }

    IEnumerator AutoFire2()
    {
        // 코루틴으로 발사 간격을 제한하는 연사 예제
        Instantiate(bullet, spPoint.position, spPoint.rotation);
        gunSound[1].Play();
        fire.SetActive(true);
        canFire = false;

        yield return new WaitForSeconds(0.1f);
        canFire = true;
    }

    IEnumerator DestroySelf()
    {
        // 탱크가 파괴되면 폭발 이펙트 후 현재 씬을 다시 불러온다.
        Instantiate(explosion, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
