using UnityEngine;

public class IgnorePlayerCollision : MonoBehaviour
{
    void Start()
    {
        // 1. 태그가 "Player"인 오브젝트를 찾습니다.
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // 2. 플레이어와 나(마커)의 충돌체(Collider)를 가져옵니다.
            Collider playerCol = player.GetComponent<Collider>();
            Collider myCol = GetComponent<Collider>();

            // 3. 둘 다 있다면 "물리적으로 부딪히지 마!"라고 설정합니다.
            if (playerCol != null && myCol != null)
            {
                Physics.IgnoreCollision(playerCol, myCol);
            }
        }
    }
}