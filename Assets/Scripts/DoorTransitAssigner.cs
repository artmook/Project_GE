using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class DoorTransitAssigner : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("할당할 문의 개수")]
    public int assignCount = 3;

    [Tooltip("이동할 목적지 정보 (씬 이름, 스폰 포인트 이름)")]
    public TransitData destinationData;

    void Start()
    {
        // GameStateManager가 준비될 때까지 대기하거나 순서상 늦게 실행되어야 함.
        // 하지만 GameStateManager는 Awake에서 인스턴스를 생성하므로 Start에서는 안전함.
        
        string currentScene = SceneManager.GetActiveScene().name;

        // 이미 이 씬에 대해 처리가 완료되었다면 중단
        if (GameStateManager.Instance.IsSceneProcessed(currentScene))
        {
            return;
        }

        AssignDestinations();
        
        // 처리 완료 표시
        GameStateManager.Instance.SetSceneProcessed(currentScene);
    }

    void AssignDestinations()
    {
        // 씬 내의 모든 문을 찾음
        OpenableDoor[] allDoors = FindObjectsByType<OpenableDoor>(FindObjectsSortMode.None);
        
        if (allDoors == null || allDoors.Length == 0) return;

        List<OpenableDoor> allDoorList = allDoors.ToList();
        List<OpenableDoor> selectedDoors = new List<OpenableDoor>();

        // 할당할 개수가 전체 문 개수보다 많거나 같으면 전체 할당
        if (assignCount >= allDoorList.Count)
        {
            selectedDoors.AddRange(allDoorList);
        }
        else
        {
            // 균등 분포 알고리즘 (Farthest Point Sampling)
            // 1. 첫 번째 문은 무작위로 선택
            OpenableDoor firstDoor = allDoorList[Random.Range(0, allDoorList.Count)];
            selectedDoors.Add(firstDoor);
            allDoorList.Remove(firstDoor);

            // 2. 목표 개수가 될 때까지 추가
            while (selectedDoors.Count < assignCount && allDoorList.Count > 0)
            {
                OpenableDoor bestCandidate = null;
                float maxMinDist = -1f;

                // 남은 후보들 중 '이미 선택된 집합'과의 최소 거리가 가장 먼 후보를 찾음
                foreach (var candidate in allDoorList)
                {
                    float minDist = float.MaxValue;
                    foreach (var selected in selectedDoors)
                    {
                        float d = Vector3.Distance(candidate.transform.position, selected.transform.position);
                        if (d < minDist)
                        {
                            minDist = d;
                        }
                    }

                    if (minDist > maxMinDist)
                    {
                        maxMinDist = minDist;
                        bestCandidate = candidate;
                    }
                }

                if (bestCandidate != null)
                {
                    selectedDoors.Add(bestCandidate);
                    allDoorList.Remove(bestCandidate);
                }
                else
                {
                    break;
                }
            }
        }

        foreach (var door in selectedDoors)
        {
            // 등록 (이미 있는 경우 건너뛰거나 덮어쓰기 정책 결정. 여기선 없는 경우만 추가)
            if (!GameStateManager.Instance.IsDoorAssigned(door.uniqueID))
            {
                GameStateManager.Instance.doorAssignments.Add(door.uniqueID, destinationData);
            }
        }
    }
}
