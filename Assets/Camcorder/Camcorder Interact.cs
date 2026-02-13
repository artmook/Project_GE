using UnityEngine;

public class CamcorderInteract : MonoBehaviour, IInteractable
{
    public string pickText = "Pick up a camcorder (F)";  // 안내 텍스트
    private PlayerController pc;

    void Start()
    {
        // 1. 플레이어 찾기
        pc = FindAnyObjectByType<PlayerController>();

        if (pc != null)
        {
            pc.worldCamcorderItem = this.gameObject;

            // 만약 이미 가지고 있다면 숨김 처리 (Destroy 아님)
            if (pc.hasCamcorder)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public string GetInteractPrompt()
    {
        return pickText;
    }

    public void Interact()
    {
        if (pc != null)
        {
            // 1. 플레이어에게 캠코더 획득 상태 부여
            pc.hasCamcorder = true;

            // 2. [중요] 삭제(Destroy)하지 않고, 비활성화(숨김)
            // 그래야 나중에 PlayerController가 다시 SetActive(true)로 켤 수 있음
            gameObject.SetActive(false);
        }
    }
}