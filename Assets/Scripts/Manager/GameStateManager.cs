using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    //씬 이동 시에도 유지되어야 할 정보를 저장할 클래스
    //싱글톤 패턴 활용하여 유일 인스턴스인 GameStateManager.Instance를 통해 접근
    public static GameStateManager Instance {get; private set;}

    //중복이 불허된 데이터셋을 통해 열기 시도한 문의 ID를 저장하고 조회
    public HashSet<string> triedDoors = new HashSet<string>();
    public HashSet<string> visitedDoors = new HashSet<string>();
    public Dictionary<string, TransitData> doorAssignments = new Dictionary<string, TransitData>();
    public bool titleShown=false;

    //각각의 문이 도착지점 ID를 지니고 있다가 씬 이동 전에 이쪽으로 전달하고
    //씬 이동 후 PlayerSpawnManager가 조회하여 도착지점을 찾고 플레이어 위치를 지정
    public string targetID;

    //현실씬 방문 후 돌아올 문의 ID
    public string doorIdToReturn;

    [Header("Item Settings")]
    public int maxBatteryCount = 3;
    public int remainingBatteries;
    private bool isBatteryInitialized = false;

    void Awake()
    {
        if(Instance==null)
        {
            Instance=this;
        }
        //EssentialSystem이라는 최상위 오브젝트에 싱글톤 패턴을 적용하였으므로
        //자식인 GameStateManager는 중복 생성을 고려하지 않아도 됨

        if (!isBatteryInitialized)
        {
            remainingBatteries = maxBatteryCount;
            isBatteryInitialized = true;
        }
    }

    public void DecreaseRemainingBattery()
    {
        if (remainingBatteries > 0)
        {
            remainingBatteries--;
            Debug.Log($"Remaining Batteries: {remainingBatteries}");
        }
    }

    public void AddTriedDoor(string id)
    {
        if (!triedDoors.Contains(id))
        {
            triedDoors.Add(id);
        }
    }

    public bool IsDoorTried(string id)
    {
        return triedDoors.Contains(id);
    }

    public void AddVisitedDoor(string id)
    {
        if (!visitedDoors.Contains(id))
        {
            visitedDoors.Add(id);
        }
    }

    public bool IsDoorVisited(string id)
    {
        return visitedDoors.Contains(id);
    }
    
    public bool IsDoorAssigned(string id)
    {
        return doorAssignments.ContainsKey(id);
    }

    public HashSet<string> processedScenes = new HashSet<string>();

    public bool IsSceneProcessed(string sceneName)
    {
        return processedScenes.Contains(sceneName);
    }

    public void SetSceneProcessed(string sceneName)
    {
        if (!processedScenes.Contains(sceneName))
        {
            processedScenes.Add(sceneName);
        }
    }
}
