using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    // 1. 이 아이템이 어떤 '설계도'를 참조할지 정합니다.
    public ItemData itemData;

    // 일방적으로 획득을 알리는 이벤트로는 획득에 실패했는지 응답받을 수 없으므로 참조를 통해 획득 구현
    protected InventoryManager inventoryManager;

    public virtual void Start()
    {
        // 최신 유니티 버전 권장 API 사용 (FindObjectOfType 대신 FindAnyObjectByType 사용)
        // FindAnyObjectByType<InventoryManager>() 자체가 컴포넌트를 반환하므로 GetComponent 불필요
        inventoryManager = InventoryManager.Instance;
    }

    // 2. 다른 스크립트(FPC)가 이 아이템의 이름을 쉽게 가져갈 수 있도록 함수를 만듭니다.
    // IInteractable 인터페이스 구현
    public string GetInteractPrompt()
    {
        if (itemData != null)
        {
            return itemData.itemName;
        }
        return "알 수 없는 아이템"; // ItemData가 할당되지 않은 경우
    }

    // IInteractable 인터페이스 구현
    public void Interact()
    {
        if (itemData != null)
        {
            // inventoryManager가 null일 경우에 대한 예외 처리 추가
            if (inventoryManager != null)
            {
                if (inventoryManager.AddItem(itemData))
                {
                    Debug.Log(itemData.itemName + " 획득"); 
                    
                    // 배터리 획득 시 쿼터 감소
                    // ItemData의 이름이나 별도 태그로 구분. 여기서는 itemName을 "Battery"로 가정하거나
                    // 더 안전하게는 ItemData에 Type enum을 두는 것이 좋으나, 
                    // 현재 요구사항상 간단히 문자열 비교 또는 프리팹 비교를 수행.
                    // 유저의 요청에는 구체적인 구분법이 없었으므로 itemName으로 1차 시도.
                    if (itemData.itemName == "베터리") 
                    {
                        GameStateManager.Instance.DecreaseRemainingBattery();
                    }

                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Fail AddItem");
                }
            }
            else
            {
                Debug.LogError("Inventory Manager not found!");
            }
        }
        else
        {
            Debug.LogError("No Item Data!");
        }
    }
}