using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public Transform defaultSpawnPoint;
    private Dictionary<string, Transform> spawnPoints_Database = new Dictionary<string, Transform>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void RegisterSpawnPoint(string id, Transform point)
    {
        if (!spawnPoints_Database.ContainsKey(id))
        {
            spawnPoints_Database.Add(id, point);
        }
        else
        {
            Debug.LogWarning(id + " ID 중복발생");
        }
    }
    void Start()
    {
        string spawnID = GameStateManager.Instance.targetID;
        if (!string.IsNullOrEmpty(spawnID))
        {
            if (spawnPoints_Database.ContainsKey(spawnID))
            {
                Transform spawnPoint = spawnPoints_Database[spawnID];
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    player.transform.position = spawnPoint.position;
                    player.transform.rotation = spawnPoint.rotation;
                }
            }
            else
            {
                Debug.LogError("'" + spawnID + "' 스폰지점 탐색실패");
            }
            GameStateManager.Instance.targetID = null;
        }
        else if(defaultSpawnPoint!=null)
        {
            Debug.LogWarning("스폰 위치 지정되지 않아 기본 위치로 이동");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = defaultSpawnPoint.position;
                player.transform.rotation = defaultSpawnPoint.rotation;
            }
        }
    }
}
