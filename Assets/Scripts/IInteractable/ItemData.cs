using UnityEngine;

[CreateAssetMenu(fileName = "New ItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;      // 아이템 이름
    public Sprite icon;          // 아이콘 이미지가 들어갈 곳

    [TextArea(3, 10)]
    public string description;   // 아이템 설명

    [TextArea(3, 5)]
    public string effectDescription; // 효과 설명 (우측 하단에 표시될 텍스트)

    // 아이템 사용 함수 (기본값: false 반환 = 사용되지 않음)
    // 배터리 같은 소모품만 이 함수를 오버라이드(재정의)해서 true를 반환하게 만들 겁니다.
    public virtual bool Use()
    {
        Debug.Log($"[ItemData] {itemName}: 지금은 사용할 수 없습니다.");
        return false;
    }
}