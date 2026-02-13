using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("스폰 지점들 (빈 오브젝트)")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Item Prefabs")]
    [Tooltip("제한된 아이템 (배터리 등)")]
    public GameObject batteryPrefab;

    [Tooltip("제한 없는 아이템 목록")]
    public List<GameObject> unlimitedItems = new List<GameObject>();

    [Header("Spawn Count")]
    public int spawnCountMin = 1;
    public int spawnCountMax = 2;

    void Start()
    {
        SpawnItems();
    }

    void SpawnItems()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("Spawn points are empty!");
            return;
        }

        List<GameObject> spawnPool = new List<GameObject>();

        // 1. 제한 없는 아이템 풀 생성 (1~2개)
        // 주의: spawnCountMax가 unlimitedItems 개수보다 많을 수 있으므로 중복 허용 여부에 따라 로직이 달라질 수 있으나,
        // 현재 요구사항은 "랜덤으로 아이템을 1~2개 선택해 아이템 생성풀 구성"이므로 중복 허용 or 리스트에서 랜덤 추출.
        // 여기서는 unlimitedItems 리스트가 있다면 그 중에서 랜덤하게 N개 뽑음.
        
        int randomCount = Random.Range(spawnCountMin, spawnCountMax + 1); // Max is exclusive in int range? No, inclusive in float, exclusive in int. Start Check.
        // Random.Range(int, int) is exclusive for max. So +1 needed.
        
        // 생성할 개수만큼 Loop
        for (int i = 0; i < randomCount; i++)
        {
            if (unlimitedItems.Count > 0)
            {
                int randomIndex = Random.Range(0, unlimitedItems.Count);
                spawnPool.Add(unlimitedItems[randomIndex]);
            }
        }

        // 2. 배터리 추가 (쿼터 확인)
        // GameStateManager의 remainingBatteries 확인
        if (GameStateManager.Instance.remainingBatteries > 0 && batteryPrefab != null)
        {
            spawnPool.Add(batteryPrefab);
            // 주의: 배터리 스폰 시 쿼터를 감소시키지 않음 (획득 시 감소)
        }

        // 3. 스폰 위치 선정 및 생성
        // 스폰 포인트 셔플
        List<Transform> shuffledPoints = new List<Transform>(spawnPoints);
        Shuffle(shuffledPoints);

        // 풀에 있는 아이템 수만큼 스폰 (단, 스폰 포인트 부족하면 중단)
        int spawnLimit = Mathf.Min(spawnPool.Count, shuffledPoints.Count);

        for (int i = 0; i < spawnLimit; i++)
        {
            Instantiate(spawnPool[i], shuffledPoints[i].position, Quaternion.identity);
        }
    }

    // Fisher-Yates Shuffle
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
