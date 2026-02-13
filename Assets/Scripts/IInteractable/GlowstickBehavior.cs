using UnityEngine;
using System.Collections;

public class GlowstickBehavior : MonoBehaviour
{
    [Header("설정")]
    public float lifeTime = 30f;      // 빛이 유지되는 시간 (초)
    public float fadeDuration = 2f;   // 빛이 꺼지는 데 걸리는 시간 (초)

    [Header("컴포넌트 연결")]
    public Light myLight;             // 제어할 자식 오브젝트의 Light 컴포넌트

    void Start()
    {
        // ---------------------------------------------------------
        // 1. 물리 충돌 설정 (플레이어만 통과하기)
        // ---------------------------------------------------------

        // "Player" 태그가 붙은 오브젝트를 찾습니다.
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // 플레이어와 나의 충돌체(Collider)를 가져옵니다.
            Collider playerCollider = player.GetComponent<Collider>();
            Collider myCollider = GetComponent<Collider>();

            // 둘 다 존재한다면, 물리 엔진에게 "서로 부딪히지 마"라고 명령합니다.
            if (playerCollider != null && myCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, myCollider);
            }
        }

        // ---------------------------------------------------------
        // 2. 수명(빛) 제어 시작
        // ---------------------------------------------------------
        if (myLight != null)
        {
            StartCoroutine(FadeOutRoutine());
        }
    }

    // 시간이 지나면 빛을 서서히 끄는 코루틴
    IEnumerator FadeOutRoutine()
    {
        // 1. 수명만큼 대기 (켜져 있는 상태)
        yield return new WaitForSeconds(lifeTime);

        // 2. 서서히 어두워지기 (Fade Out)
        float startIntensity = myLight.intensity;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // Lerp를 이용해 현재 밝기에서 0까지 부드럽게 줄임
            myLight.intensity = Mathf.Lerp(startIntensity, 0f, timer / fadeDuration);
            yield return null; // 한 프레임 대기
        }

        // 3. 완전히 꺼짐
        myLight.intensity = 0f;
        myLight.enabled = false;

        // (선택 사항) 다 쓴 야광봉을 아예 삭제하고 싶다면 아래 주석 해제
        // Destroy(gameObject); 
    }
}