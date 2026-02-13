using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // 어디서든 접근 가능하게 싱글톤 처리

    [Header("설정")]
    public int maxSlots = 12; // 슬롯 최대 개수
    public List<ItemData> items = new List<ItemData>(); // 실제 아이템이 담기는 리스트

    // 인벤토리가 변할 때 UI에게 "화면 갱신해!"라고 알리는 신호
    public event Action OnInventoryChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 아이템 추가
    public bool AddItem(ItemData item)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("가방이 꽉 찼습니다.");
            return false;
        }

        items.Add(item);
        OnInventoryChanged?.Invoke(); // UI 갱신 신호 발송
        return true;
    }

    // 아이템 제거
    public void RemoveItem(ItemData item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            OnInventoryChanged?.Invoke(); // UI 갱신 신호 발송
        }
    }

    // [중요] 문 열 때 Key가 있는지 검사하는 함수
    public bool HasItem(string _itemName)
    {
        // 리스트 안에 이름이 똑같은 아이템이 하나라도 있으면 true
        return items.Exists(x => x.itemName == _itemName);
    }
}