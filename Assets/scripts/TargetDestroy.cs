using System.Collections;
using UnityEngine;

// 타겟이 총알에 맞았을 때 폭발, 점수 지급, 숨김, 리스폰을 처리한다.
// 여러 자식 Renderer/Collider가 있는 Archery Target, Dummy Target FREE에도 맞게 동작한다.
public class TargetDestroy : MonoBehaviour
{
    // 맞았을 때 생성할 폭발 효과와 다시 나타나기까지 걸리는 시간
    public Transform explosion;
    public float respawnDelay = 3f;

    // 이 타겟을 파괴했을 때 얻는 점수
    public int scoreValue = 10;

    Renderer[] targetRenderers;
    Collider[] targetColliders;
    bool isRespawning;

    // 이동 스크립트가 리스폰 중에는 움직이지 않도록 읽는 값
    public bool IsRespawning => isRespawning;

    void Awake()
    {
        CacheTargetParts();
    }

    void OnEnable()
    {
        CacheTargetParts();

        if (!isRespawning)
        {
            RestoreTargetParts();
        }
    }

    public void DestroySelf(Vector3 hitPosition)
    {
        // 이미 사라진 상태라면 같은 타겟에서 점수가 여러 번 들어가지 않게 막는다.
        if (isRespawning)
        {
            return;
        }

        CacheTargetParts();
        isRespawning = true;

        if (explosion != null)
        {
            Instantiate(explosion, hitPosition, Quaternion.identity);
        }

        ScoreManager.AddScore(scoreValue);
        HideTargetParts();
        StartCoroutine(RespawnAfterDelay());
    }

    void CacheTargetParts()
    {
        targetRenderers = GetComponentsInChildren<Renderer>(true);
        targetColliders = GetComponentsInChildren<Collider>(true);
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        RestoreTargetParts();
        isRespawning = false;
    }

    void HideTargetParts()
    {
        SetRenderersEnabled(false);
        SetCollidersEnabled(false);
    }

    void RestoreTargetParts()
    {
        SetRenderersEnabled(true);
        SetCollidersEnabled(true);
    }

    void SetRenderersEnabled(bool enabled)
    {
        if (targetRenderers == null)
        {
            return;
        }

        foreach (Renderer targetRenderer in targetRenderers)
        {
            if (targetRenderer != null)
            {
                targetRenderer.enabled = enabled;
            }
        }
    }

    void SetCollidersEnabled(bool enabled)
    {
        if (targetColliders == null)
        {
            return;
        }

        foreach (Collider targetCollider in targetColliders)
        {
            if (targetCollider != null)
            {
                targetCollider.enabled = enabled;
            }
        }
    }
}
