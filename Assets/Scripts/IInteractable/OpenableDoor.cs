using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenableDoor : MonoBehaviour, IInteractable
{
    [HideInInspector]
    public string uniqueID;
    public string interactString = "Door Open(F)";
    
    //이번 게임 내내 잠긴 문으로 사용할 때 표시할 텍스트
    public string lockedString = "Locked";
    //방문 후 재 이동 시도시 표시할 텍스트
    public string alreadyVisitedString = "Locked from inside";

    public GameObject doorObj;
    public GameObject doorEffectObj;
    public TransitData transit;
    public Transform mySpawnPoint;

    [Header("상호작용 피드백")]
    public AudioClip lockedSound;
    private AudioSource audioSource;
    
    void Awake()
    {
        string sceneName=SceneManager.GetActiveScene().name;
        string posStr=transform.position.ToString("F1");
        uniqueID=$"{sceneName}_{posStr}";
        PlayerSpawnManager spawnManager= FindAnyObjectByType<PlayerSpawnManager>();
        if(spawnManager!=null&mySpawnPoint!=null) spawnManager.RegisterSpawnPoint(uniqueID,mySpawnPoint);
        
        
    }
    void Start()
    {
        // 씬 로드 시 할당 여부 확인
        if (GameStateManager.Instance.IsDoorAssigned(uniqueID))
        {
            transit = GameStateManager.Instance.doorAssignments[uniqueID];
        }
        else
        {
            // 로컬에 할당된 정보가 있다면 등록
            if (!(string.IsNullOrEmpty(transit.sceneToMove) || string.IsNullOrEmpty(transit.destinationObj)))
            {
                GameStateManager.Instance.doorAssignments.Add(uniqueID, transit);
            }
        }
        if(doorEffectObj!=null) doorEffectObj.SetActive(GameStateManager.Instance.IsDoorAssigned(uniqueID));
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    //코드를 통해 목적지를 할당할 수 있도록하는 함수
    public void AssignTransit(TransitData t)
    {
        transit=t;
    }
    public string GetInteractPrompt()
    {
        //열기를 시도한 문일때 텍스트를 변경
        if (GameStateManager.Instance.IsDoorTried(uniqueID))
        {
            //방문한 문이라면 방문했던 문이 더이상 열리지 않는다고 표시
            if (GameStateManager.Instance.IsDoorVisited(uniqueID))
            {
                return alreadyVisitedString;
            }
            else //열어봤지만 방문하지 못한 문이라면 잠긴 문이라는 뜻
            {
                return lockedString;
            }
        }
        return interactString;
    }

    public void Interact()
    {
        //문에 목적지가 할당된 상태라면
        if (GameStateManager.Instance.IsDoorAssigned(uniqueID))
        {
            // transit 데이터 최신화
            transit = GameStateManager.Instance.doorAssignments[uniqueID];

            //방문한 적이 없다면 이동
            if (!GameStateManager.Instance.IsDoorVisited(uniqueID))
            {
                VisionManager.Instance.CantSeeMonster();
                GameStateManager.Instance.AddVisitedDoor(uniqueID);
                GameStateManager.Instance.targetID=transit.destinationObj;
                GameStateManager.Instance.doorIdToReturn=uniqueID;
                SceneManager.LoadScene(transit.sceneToMove);
            }
            else //방문한 적이 있다면
            {
                //재방문 최초 시도시
                if (!GameStateManager.Instance.IsDoorTried(uniqueID))
                {
                    //잠김 효과
                    if (lockedSound != null)
                    {
                        audioSource.PlayOneShot(lockedSound);
                    }
                    //열기 시도한 문으로 등록
                    GameStateManager.Instance.AddTriedDoor(uniqueID);
                }
            }
        }
        //목적지가 없으면 잠긴 문
        else
        {
            //열어본 적이 없다면
            if (!GameStateManager.Instance.IsDoorTried(uniqueID))
            {
                if (lockedSound != null)
                {
                    audioSource.PlayOneShot(lockedSound);
                }
                //열기 시도한 문으로 등록
                GameStateManager.Instance.AddTriedDoor(uniqueID);
            }
        }
    }
}