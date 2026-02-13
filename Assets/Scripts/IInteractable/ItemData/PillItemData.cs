using UnityEngine;

// [CreateAssetMenu]는 반드시 class 바로 위에 있어야 합니다.
[CreateAssetMenu(fileName = "New Pill Data", menuName = "Inventory/Pill Item Data")]
public class PillItemData : ItemData  // 여기서 { 가 열려야 합니다.
{
    [Header("알약 설정")]
    public float effectDuration = 180f; // 효과 지속 시간 (초)

    // Use 함수도 class의 { } 안쪽에 있어야 합니다.
    public override bool Use()
    {
        // 1. 씬에 있는 VisionManager를 찾습니다.
        // (최신 유니티는 FindAnyObjectByType, 구버전은 FindObjectOfType 사용)
        VisionManager vision = FindAnyObjectByType<VisionManager>();

        // 2. VisionManager가 있으면 무조건 사용 성공 (몬스터 없어도 OK)
        if (vision != null)
        {
            // 알약 효과 발동 (시간 전달)
            vision.ActivatePillEffect(effectDuration);
            Debug.Log($"💊 알약 꿀꺽! {effectDuration}초 동안 영안(True Sight) 개방!");

            return true; // true = 아이템 사용 성공 (인벤토리에서 사라짐)
        }
        else
        {
            Debug.LogWarning("VisionManager가 씬에 없습니다! (EssentialSystem에 붙어있는지 확인하세요)");
            return false; // 사용 실패 (안 사라짐)
        }
    }
} // 여기서 class가 끝나야 합니다.