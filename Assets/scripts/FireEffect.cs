using System.Collections;
using UnityEngine;

// 발사 화염 오브젝트를 잠깐 보여준 뒤 자동으로 꺼주는 스크립트.
// FireEffect 오브젝트가 SetActive(true) 되면 OnEnable이 호출되고, 0.1초 뒤 다시 비활성화된다.
public class FireEffect : MonoBehaviour
{
    void OnEnable()
    {
        StartCoroutine(Disable(0.1f));
    }

    // 필요할 때 직접 호출할 수 있는 즉시 비활성화 함수
    void Disable()
    {
        gameObject.SetActive(false);
    }

    // delay초 동안 기다렸다가 발사 화염을 꺼준다.
    IEnumerator Disable(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
