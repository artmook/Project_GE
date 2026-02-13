using UnityEngine;
using System.Collections;

public class VisionManager : MonoBehaviour
{
    [Header("연결")]
    public Camera mainCamera;
    public PlayerController player;

    [Header("설정")]
    public string hiddenLayerName = "HiddenMonster";

    private int defaultMask;
    private int trueSightMask;
    private int hiddenLayerIndex;

    private bool isPillActive = false;
    public bool isDead = false; // 💀 죽었는지 확인하는 변수 추가

    public static VisionManager Instance { get; private set; }

    void Awake(){
        if(Instance!=null){
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        
        if (mainCamera == null) mainCamera = Camera.main;

        hiddenLayerIndex = LayerMask.NameToLayer(hiddenLayerName);
        defaultMask = mainCamera.cullingMask & ~(1 << hiddenLayerIndex);
        trueSightMask = defaultMask | (1 << hiddenLayerIndex);

        mainCamera.cullingMask = defaultMask;
    }

    void Update()
    {
        if (player == null) return;

        // [수정됨] 죽었으면 무조건 보임! (나머지 조건 무시)
        if (isDead)
        {
            mainCamera.cullingMask = trueSightMask;
            return;
        }

        // 평소 로직 (캠코더 or 알약)
        bool canSeeMonster = player.camcorderOn || isPillActive;

        if (canSeeMonster)
        {
            mainCamera.cullingMask = trueSightMask;
        }
        else
        {
            mainCamera.cullingMask = defaultMask;
        }
    }

    public void CantSeeMonster(){
        mainCamera.cullingMask = defaultMask;
    }
    // 💀 데드신 시작할 때 호출할 함수 (외부에서 부름)
    public void ForceReveal()
    {
        isDead = true; // 죽음 상태 ON
        mainCamera.cullingMask = trueSightMask; // 강제로 보이게 전환
        Debug.Log("💀 사망: 몬스터 강제 노출");
    }

    public void ActivatePillEffect(float duration)
    {
        if (!isDead) StartCoroutine(PillRoutine(duration));
    }

    IEnumerator PillRoutine(float duration)
    {
        isPillActive = true;
        yield return new WaitForSeconds(duration);
        isPillActive = false;
    }
}