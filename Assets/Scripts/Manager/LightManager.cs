using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public Transform playerTr;
    public List<GameObject> bulbs;

    public float blinkRadius = 20f;
    public float proximityUpdateInterval = 0.5f;
    public float minTimeBetweenBlinks = 0.1f;
    public float maxTimeBetweenBlinks = 0.5f;
    public float minBlinkDuration = 0.1f;
    public float maxBlinkDuration = 0.3f;

    List<Renderer> allBulbRenderers = new List<Renderer>();
    List<Renderer> nearbyBulbs = new List<Renderer>();

    private Dictionary<Renderer, Material> bulbMaterialMap = new Dictionary<Renderer, Material>();
    private HashSet<Renderer> currentlyBlinking = new HashSet<Renderer>();
    private Color emissionColor;
    private readonly int emissionColorID = Shader.PropertyToID("_EmissionColor");


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTr=GameObject.FindGameObjectWithTag("Player").transform;
        foreach (GameObject bulb in bulbs)
        {
            Renderer r = bulb.GetComponent<Renderer>();
            if (r != null)
            {
                allBulbRenderers.Add(r);
                Material mat = r.material; // 고유 머티리얼 생성
                bulbMaterialMap.Add(r, mat);

                if (emissionColor == default && mat.HasProperty(emissionColorID))
                {
                    emissionColor = mat.GetColor(emissionColorID);
                }
            }
        }
        StartCoroutine(UpdateNearbyBulbsRoutine()); // (주변 전구 갱신용)
        StartCoroutine(BlinkScheduler());
    }
    IEnumerator UpdateNearbyBulbsRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(proximityUpdateInterval);

        while (true)
        {
            // 플레이어가 할당되지 않았으면 대기
            if (playerTr == null)
            {
                Debug.LogWarning("플레이어가 할당되지 않아 깜빡임을 중지합니다.");
                yield return wait;
                continue;
            }

            nearbyBulbs.Clear(); // 주변 리스트 초기화
            Vector3 playerPos = playerTr.position;
            // (최적화) Vector3.Distance 대신 제곱근 비교(sqrMagnitude) 사용
            float radiusSquared = blinkRadius * blinkRadius;

            // 126개의 모든 전구를 순회
            foreach (Renderer bulb in allBulbRenderers)
            {
                // 전구와 플레이어 사이의 거리를 계산
                if ((bulb.transform.position - playerPos).sqrMagnitude <= radiusSquared)
                {
                    nearbyBulbs.Add(bulb); // 가까우면 리스트에 추가
                }
            }

            yield return wait; // 설정된 갱신 주기(0.5초)만큼 대기
        }
    }
    IEnumerator BlinkScheduler()
    {
        while (true)
        {
            // 1. 다음 깜빡임까지 대기
            float waitTime = Random.Range(minTimeBetweenBlinks, maxTimeBetweenBlinks);
            yield return new WaitForSeconds(waitTime);

            // 2. ◀◀◀ 핵심 변경: 'nearbyBulbs' 리스트에 전구가 있을 때만 실행
            if (nearbyBulbs.Count > 0)
            {
                // 3. '주변 전구' 리스트에서 무작위로 하나 선택
                int index = Random.Range(0, nearbyBulbs.Count);
                Renderer targetBulb = nearbyBulbs[index];

                // 4. (안전 장치) 이미 깜빡이는 중이 아니라면 실행
                if (!currentlyBlinking.Contains(targetBulb))
                {
                    StartCoroutine(BlinkOneBulb(targetBulb));
                }
            }
        }
    }
    IEnumerator BlinkOneBulb(Renderer targetBulb)
    {
        currentlyBlinking.Add(targetBulb);
        Material mat = bulbMaterialMap[targetBulb];

        // 끄기
        mat.DisableKeyword("_EMISSION");
        mat.SetColor(emissionColorID, Color.black);

        float blinkDuration = Random.Range(minBlinkDuration, maxBlinkDuration);
        yield return new WaitForSeconds(blinkDuration);

        // 다시 켜기
        mat.EnableKeyword("_EMISSION");
        mat.SetColor(emissionColorID, emissionColor);

        currentlyBlinking.Remove(targetBulb);
    }
}
