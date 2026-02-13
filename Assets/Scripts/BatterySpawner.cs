using UnityEngine;

public class BatterySpawner : MonoBehaviour
{
    public GameObject batteryPrefab;       // 생성할 배터리 프리팹
    public Transform[] spawnPoints;        // 스폰 위치들

    void Start()
    {
        SpawnBattery();
    }

    void SpawnBattery()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length); // 랜덤 인덱스 선택
        Transform spawnPoint = spawnPoints[randomIndex];       // 해당 위치 가져오기

        Instantiate(batteryPrefab, spawnPoint.position, Quaternion.identity); // 아이템 생성
    }
}