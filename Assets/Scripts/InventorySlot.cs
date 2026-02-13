using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;         // 아이콘 이미지
    public Button slotButton;  // 버튼 컴포넌트

    private ItemData myItem;   // 내가 가지고 있는 아이템 데이터
    private InventoryUI uiManager; // 나를 관리하는 UI 매니저


    public void Setup(ItemData item, InventoryUI ui)
    {
        myItem = item;
        uiManager = ui;

        // 아이템 정보가 있으면 아이콘 띄우기
        if (myItem != null)
        {
            icon.sprite = myItem.icon;
            icon.enabled = true;
        }
        else
        {
            icon.enabled = false;
        }

        // 버튼 클릭 시 "나 클릭됐어요!" 하고 UI 매니저에게 보고
        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => {
                if (uiManager != null) uiManager.SelectItem(myItem);
            });
        }
    }
}