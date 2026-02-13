using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("새로 만든 인벤토리 연결")]
    public InventoryUI inventoryUI;

    [Header("HUD")]
    public GameObject crosshair;
    public TextMeshProUGUI itemNameText;

    public bool isInventoryOpen = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 텍스트 초기화
        if (itemNameText != null) itemNameText.text = "";

        // [수정] 시작 시 크로스헤어 상태를 강제로 설정하지 않음.
        // GameManager가 알아서 제어하게 둠.
    }

    public void ToggleInventory()
    {
        if (inventoryUI == null) return;

        inventoryUI.ToggleInventory();
        isInventoryOpen = inventoryUI.inventoryPanel.activeSelf;

        // 인벤토리 열리면 크로스헤어 끄기
        if (crosshair != null && isInventoryOpen)
        {
            crosshair.SetActive(false);
        }
    }

    // 외부(GameManager)에서 크로스헤어를 켜고 끄기 위한 함수 추가
    public void SetCrosshair(bool state)
    {
        if (crosshair != null) crosshair.SetActive(state);
    }

    public void ShowItemNameText(string text)
    {
        if (itemNameText != null) itemNameText.text = text;
    }

    public void ClearItemNameText()
    {
        if (itemNameText != null) itemNameText.text = "";
    }
}