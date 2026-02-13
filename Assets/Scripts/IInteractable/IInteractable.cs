using UnityEngine;

// 상호작용 가능한 오브젝트 인터페이스
public interface IInteractable
{
    // 상호작용 동작 정의
    void Interact();

    // UI에 표시할 텍스트 반환 (예: "열기", "줍기")
    string GetInteractPrompt();
}