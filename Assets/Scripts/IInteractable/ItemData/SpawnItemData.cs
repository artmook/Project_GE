using UnityEngine;

// 일반 ItemData를 상속받아 기능을 확장함
[CreateAssetMenu(fileName = "New Spawn Item", menuName = "Inventory/Spawn Item Data")]
public class SpawnItemData : ItemData
{
    [Header("소환 설정")]
    public GameObject prefabToSpawn; // 낳을 물건 (야광봉 or 마커)

    // "사용" 했을 때 일어날 일 (플레이어 코드에 if문 추가 없이 여기서 처리)
    public override bool Use()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        // 1. 플레이어 앞쪽 위치 계산
        Vector3 spawnPos = player.transform.position + (player.transform.forward * 0.5f) + (Vector3.up * 1.0f);

        // 2. 물건 소환
        Instantiate(prefabToSpawn, spawnPos, Random.rotation);

        Debug.Log($"{itemName} 사용됨: 바닥에 생성");

        return true; // true = 소모됨(인벤에서 삭제)
    }
}