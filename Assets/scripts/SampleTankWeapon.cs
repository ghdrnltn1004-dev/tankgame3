using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 플레이어 탱크의 무기/피격/파괴를 담당한다.
// 단발, 연사, 발사 효과, 사운드, 탱크가 맞았을 때의 피격 효과와 체력 감소까지 여기서 처리한다.
public class SampleTankWeapon : MonoBehaviour
{
    // 발사와 이펙트에 사용할 프리팹/사운드
    public Transform bulletPrefab;
    public Transform muzzleFlashPrefab;
    public Transform impactEffectPrefab;
    public Transform destroyEffectPrefab;
    public AudioClip singleShotClip;
    public AudioClip autoShotClip;

    // 포구 위치와 연사 간격
    public Vector3 muzzleOffset = new Vector3(0f, 0.55f, 1.35f);
    public float fireInterval = 0.1f;

    // 탱크 체력과 맞았을 때 빨갛게 번쩍이는 시간
    public int hitPoints = 10;
    public float hitFlashDuration = 0.14f;

    // 런타임에 필요한 내부 상태
    private AudioSource gunAudioSource;
    private Renderer[] tankRenderers;
    private Material[][] tankMaterials;
    private Color[][] originalColors;
    private Coroutine hitFlashRoutine;
    private float nextAutoFireTime;
    private bool isDestroyed;

    void Awake()
    {
        // 맞았을 때 색을 잠깐 바꿨다가 되돌리기 위해 원래 재질 색을 저장한다.
        CacheTankMaterials();
    }

    void Update()
    {
        if (isDestroyed)
        {
            return;
        }

        // 마우스 왼쪽을 처음 누르면 단발 사운드로 1발 발사한다.
        if (Input.GetButtonDown("Fire1"))
        {
            Fire(singleShotClip);
            nextAutoFireTime = Time.time + fireInterval;
        }

        // 마우스 왼쪽을 누르고 있거나 LeftShift를 누르면 연사 사운드로 계속 발사한다.
        if (IsAutoFireHeld() && Time.time >= nextAutoFireTime)
        {
            Fire(autoShotClip);
            nextAutoFireTime = Time.time + fireInterval;
        }
    }

    bool IsAutoFireHeld()
    {
        return Input.GetButton("Fire1") || Input.GetKey(KeyCode.LeftShift);
    }

    void OnTriggerEnter(Collider other)
    {
        // 상대 총알이 Trigger로 들어오는 경우를 처리한다.
        TryTakeBulletHit(other.gameObject, other.transform.position);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 상대 총알이 일반 Collision으로 부딪히는 경우도 처리한다.
        Vector3 hitPosition = collision.contactCount > 0 ? collision.GetContact(0).point : collision.transform.position;
        TryTakeBulletHit(collision.gameObject, hitPosition);
    }

    void TryTakeBulletHit(GameObject bulletObject, Vector3 hitPosition)
    {
        if (isDestroyed || bulletObject == null || !bulletObject.CompareTag("Bullet"))
        {
            return;
        }

        // 맞은 위치에 피격 이펙트를 띄우고 탱크 몸체를 짧게 빨갛게 만든다.
        SpawnEffect(impactEffectPrefab, hitPosition, Quaternion.identity, 2f);
        FlashTankBody();

        hitPoints--;
        if (hitPoints <= 0)
        {
            StartCoroutine(DestroyTank());
        }

        Destroy(bulletObject);
    }

    void Fire(AudioClip shotClip)
    {
        if (bulletPrefab == null)
        {
            return;
        }

        // 탱크 기준 muzzleOffset 위치를 실제 월드 좌표로 바꿔 총알을 만든다.
        Vector3 muzzlePosition = transform.TransformPoint(muzzleOffset);
        Transform bullet = Instantiate(bulletPrefab, muzzlePosition, transform.rotation);

        // 방금 만든 총알이 자기 탱크와 바로 충돌하지 않도록 무시한다.
        IgnoreTankCollisions(bullet);

        SpawnEffect(muzzleFlashPrefab, muzzlePosition, transform.rotation, 0.35f);

        if (shotClip != null)
        {
            GetGunAudioSource().PlayOneShot(shotClip);
        }
    }

    AudioSource GetGunAudioSource()
    {
        // AudioSource가 없으면 실행 중에 자동으로 붙인다.
        if (gunAudioSource == null)
        {
            gunAudioSource = gameObject.AddComponent<AudioSource>();
            gunAudioSource.playOnAwake = false;
            gunAudioSource.spatialBlend = 1f;
        }

        return gunAudioSource;
    }

    void IgnoreTankCollisions(Transform bullet)
    {
        Collider bulletCollider = bullet.GetComponent<Collider>();
        if (bulletCollider == null)
        {
            return;
        }

        // 탱크 본체/자식 콜라이더 모두와 총알 충돌을 무시한다.
        Collider[] tankColliders = GetComponentsInChildren<Collider>();
        foreach (Collider tankCollider in tankColliders)
        {
            Physics.IgnoreCollision(bulletCollider, tankCollider);
        }
    }

    void CacheTankMaterials()
    {
        tankRenderers = GetComponentsInChildren<Renderer>();
        tankMaterials = new Material[tankRenderers.Length][];
        originalColors = new Color[tankRenderers.Length][];

        for (int i = 0; i < tankRenderers.Length; i++)
        {
            tankMaterials[i] = tankRenderers[i].materials;
            originalColors[i] = new Color[tankMaterials[i].Length];

            for (int j = 0; j < tankMaterials[i].Length; j++)
            {
                originalColors[i][j] = GetMaterialColor(tankMaterials[i][j]);
            }
        }
    }

    void FlashTankBody()
    {
        // 연속으로 맞으면 이전 점멸 코루틴을 멈추고 새로 시작한다.
        if (hitFlashRoutine != null)
        {
            StopCoroutine(hitFlashRoutine);
        }

        hitFlashRoutine = StartCoroutine(FlashTankBodyRoutine());
    }

    IEnumerator FlashTankBodyRoutine()
    {
        Color hitColor = new Color(1f, 0.12f, 0.06f, 1f);

        // 모든 탱크 재질을 빨간색에 가깝게 바꿔 피격을 보여준다.
        for (int i = 0; i < tankMaterials.Length; i++)
        {
            for (int j = 0; j < tankMaterials[i].Length; j++)
            {
                SetMaterialColor(tankMaterials[i][j], Color.Lerp(originalColors[i][j], hitColor, 0.75f));
            }
        }

        yield return new WaitForSeconds(hitFlashDuration);

        RestoreTankColors();
        hitFlashRoutine = null;
    }

    void RestoreTankColors()
    {
        if (tankMaterials == null || originalColors == null)
        {
            return;
        }

        // 저장해 둔 원래 색으로 탱크 재질을 되돌린다.
        for (int i = 0; i < tankMaterials.Length; i++)
        {
            for (int j = 0; j < tankMaterials[i].Length; j++)
            {
                SetMaterialColor(tankMaterials[i][j], originalColors[i][j]);
            }
        }
    }

    Color GetMaterialColor(Material material)
    {
        // URP 재질은 _BaseColor, 기본 재질은 _Color를 사용한다.
        if (material != null && material.HasProperty("_BaseColor"))
        {
            return material.GetColor("_BaseColor");
        }

        if (material != null && material.HasProperty("_Color"))
        {
            return material.GetColor("_Color");
        }

        return Color.white;
    }

    void SetMaterialColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    void SpawnEffect(Transform effectPrefab, Vector3 position, Quaternion rotation, float lifeTime)
    {
        if (effectPrefab == null)
        {
            return;
        }

        Transform effect = Instantiate(effectPrefab, position, rotation);
        Destroy(effect.gameObject, lifeTime);
    }

    IEnumerator DestroyTank()
    {
        isDestroyed = true;

        // 파괴 순간에는 조작과 충돌을 꺼서 더 이상 움직이거나 맞지 않게 한다.
        RestoreTankColors();
        DisableTankControlsAndColliders();
        SpawnEffect(destroyEffectPrefab, transform.position + Vector3.up * 0.8f, Quaternion.identity, 2f);

        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void DisableTankControlsAndColliders()
    {
        // 이 스크립트를 제외한 탱크 조작 스크립트를 끈다.
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour != this)
            {
                behaviour.enabled = false;
            }
        }

        // 파괴된 탱크에 추가 총알이 맞지 않도록 콜라이더도 끈다.
        Collider[] tankColliders = GetComponentsInChildren<Collider>();
        foreach (Collider tankCollider in tankColliders)
        {
            tankCollider.enabled = false;
        }
    }
}
