using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // 리스트 사용을 위해 추가

public class InventoryUI : MonoBehaviour
{
    [Header("전체 패널")]
    public GameObject inventoryPanel;

    [Header("좌측: 아이템 목록")]
    public Transform slotParent;
    public GameObject slotPrefab;

    [Header("우측: 상세 정보")]
    public GameObject rightPanel;
    public Image detailIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI effectText;
    public Button useButton;

    private ItemData currentItem;
    private List<InventorySlot> createdSlots = new List<InventorySlot>(); // 만들어진 슬롯들을 기억할 리스트

    void Start()
    {
        // 1. 매니저 연결
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateSlotUI;
        }

        // 2. 버튼 연결
        if (useButton != null) useButton.onClick.AddListener(OnUseClick);

        // 3. [중요] 시작하자마자 빈 슬롯들을 미리 다 생성합니다!
        CreateSlots();

        // 4. 초기화
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (rightPanel != null) rightPanel.SetActive(false);
    }

    // 슬롯을 최초 1회 생성하는 함수
    void CreateSlots()
    {
        if (slotParent == null || slotPrefab == null || InventoryManager.Instance == null) return;

        // 기존에 있던 거 싹 지우기 (혹시 모르니)
        foreach (Transform child in slotParent) Destroy(child.gameObject);
        createdSlots.Clear();

        // 최대 개수만큼 빈 슬롯 생성
        for (int i = 0; i < InventoryManager.Instance.maxSlots; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotParent);
            InventorySlot slotScript = newSlot.GetComponent<InventorySlot>();

            // 생성된 슬롯을 리스트에 저장해둠 (나중에 내용만 바꾸려고)
            if (slotScript != null)
            {
                slotScript.Setup(null, this); // 처음엔 빈 상태로 설정
                createdSlots.Add(slotScript);
            }
        }
    }

    public void ToggleInventory()
    {
        Debug.Log("3. InventoryUI 도착! 문 열 준비 완료."); // CCTV 1

        if (inventoryPanel == null)
        {
            Debug.LogError("🚨 비상! Inventory Panel이 연결되어 있지 않습니다!"); // CCTV 2 (범인 검거용)
            return;
        }

        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        Debug.Log($"4. 패널 상태 변경됨: {(isActive ? "켜짐(ON)" : "꺼짐(OFF)")}"); // CCTV 3

        if (isActive)
        {
            UpdateSlotUI();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (rightPanel != null) rightPanel.SetActive(false);
            // currentItem = null; // (잠시 주석: 오류 방지)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // 데이터에 맞춰 슬롯 '내용'만 갱신하는 함수
    void UpdateSlotUI()
    {
        if (InventoryManager.Instance == null) return;

        // 모든 슬롯을 순회하면서
        for (int i = 0; i < createdSlots.Count; i++)
        {
            // 내 인벤토리에 아이템이 i번째에 존재한다면?
            if (i < InventoryManager.Instance.items.Count)
            {
                // 그 아이템을 보여줌
                createdSlots[i].Setup(InventoryManager.Instance.items[i], this);
            }
            else
            {
                // 없으면 빈 칸으로 만듦
                createdSlots[i].Setup(null, this);
            }
        }
    }

    public void SelectItem(ItemData item)
    {
        if (item == null) return; // 빈 슬롯 클릭하면 무시

        currentItem = item;
        if (rightPanel != null) rightPanel.SetActive(true);

        if (detailIcon != null)
        {
            detailIcon.sprite = item.icon;
            detailIcon.preserveAspect = true;
        }
        if (nameText != null) nameText.text = item.itemName;
        if (descText != null) descText.text = item.description;
        if (effectText != null) effectText.text = item.effectDescription;
    }

    void OnUseClick()
    {
        if (currentItem != null)
        {
            if (currentItem.Use())
            {
                InventoryManager.Instance.RemoveItem(currentItem);
                if (rightPanel != null) rightPanel.SetActive(false);
                currentItem = null;
            }
        }
    }
}