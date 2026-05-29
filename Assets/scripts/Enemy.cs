using System.Collections;
using UnityEngine;

// 자동 터렛 적군.
// 탱크가 탐지 거리 안에 들어오면 포탑을 돌려 조준하고, 사정거리 안이면 자동으로 발사한다.
public class Enemy : MonoBehaviour
{
    // SampleScene에서 연결되는 총알/폭발 프리팹
    public Transform bullet;
    public Transform explosion;

    // 터렛이 추적하고 발사할 때 사용하는 내부 참조들
    Transform tank;
    Transform turret;
    Transform spPoint;
    Transform fire;
    AudioSource gunSound;

    // 탱크를 감지하는 거리와 실제 발사 가능한 거리
    const float RADAR_DIST = 12f;
    const float FIRE_DIST = 10f;

    bool canFire = true;

    void Start()
    {
        InitGame();
    }

    void Update()
    {
        if (tank == null)
        {
            return;
        }

        // 탱크와 터렛 사이의 방향/거리 계산
        Vector3 delta = tank.position - transform.position;
        float dist = delta.magnitude;

        // 탐지 거리 안이면 포탑을 부드럽게 탱크 방향으로 회전한다.
        if (dist <= RADAR_DIST)
        {
            Quaternion rot = Quaternion.LookRotation(delta);
            turret.rotation = Quaternion.Slerp(turret.rotation, rot, 5f * Time.deltaTime);
        }

        // 발사 거리 안이면 일정 간격으로 자동 발사한다.
        if (dist <= FIRE_DIST && canFire)
        {
            StartCoroutine(AutoFire());
        }

        if (dist > FIRE_DIST)
        {
            gunSound.Stop();
        }
    }

    void InitGame()
    {
        GameObject tankObject = GameObject.Find("Tank");
        if (tankObject != null)
        {
            tank = tankObject.transform;
        }

        turret = transform.Find("Turret");
        spPoint = transform.Find("Turret/SpPoint");

        // 발사 화염은 쏠 때만 잠깐 켠다.
        fire = transform.Find("Turret/Fire");
        fire.gameObject.SetActive(false);

        gunSound = GetComponent<AudioSource>();
    }

    IEnumerator AutoFire()
    {
        if (bullet != null && spPoint != null)
        {
            Instantiate(bullet, spPoint.position, spPoint.rotation);
        }

        if (fire != null)
        {
            fire.gameObject.SetActive(true);
        }

        if (gunSound != null)
        {
            gunSound.Play();
        }

        canFire = false;
        yield return new WaitForSeconds(0.2f);
        canFire = true;
    }

    // Bullet.cs에서 Enemy 태그를 맞혔을 때 SendMessage로 호출된다.
    void DestroySelf(Vector3 pos)
    {
        if (explosion != null)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }

        StartCoroutine(DestroyLazy());
    }

    IEnumerator DestroyLazy()
    {
        // 터렛 부품들의 재질을 점점 투명하게 만든 뒤 오브젝트를 제거한다.
        Material mat1 = turret.GetComponent<Renderer>().material;
        Material mat2 = transform.Find("Base").GetComponent<Renderer>().material;
        Material mat3 = transform.Find("Turret/Barrel").GetComponent<Renderer>().material;
        Color color = mat1.color;

        for (float alpha = 1f; alpha >= 0f; alpha -= 0.02f)
        {
            color.a = alpha;
            mat1.color = color;
            mat2.color = color;
            mat3.color = color;
            yield return null;
        }

        Destroy(gameObject);
    }
}
