using UnityEngine;
using System.Collections;

public class KeyDoor : MonoBehaviour, IInteractable
{
    [HideInInspector]
    public string uniqueID;

    public string interactString = "Door Open(F)";
    public string needKeyString = "Need a Key";

    public bool requiresKey = true;

    [Header("문 오브젝트 설정")]
    public GameObject doorObj;

    [Header("상호작용 피드백")]
    public AudioClip openSound;    //문 열리는 소리
    public AudioClip lockedSound;
    private AudioSource audioSource;

    private bool isOpened = false;

    void Awake()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string posStr = transform.position.ToString("F1");
        uniqueID = $"{sceneName}_{posStr}";
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public string GetInteractPrompt()
    {
        if (isOpened)
            return "";

        if (requiresKey && GameStateManager.Instance.IsDoorTried(uniqueID))
            return needKeyString;

        return interactString;
    }

    public void Interact()
    {
        if (isOpened)
            return;

        // 키 필요 + 키 없음
        if (requiresKey && !InventoryManager.Instance.HasItem("Key"))
        {
            Debug.Log("Need a Key");

            if (lockedSound != null)
                audioSource.PlayOneShot(lockedSound);

            GameStateManager.Instance.AddTriedDoor(uniqueID);
            return;
        }

        // 키 있음 → 문 열림
        if (requiresKey && InventoryManager.Instance.HasItem("Key"))
        {
            Debug.Log("Open");

            isOpened = true;

            StartCoroutine(OpenDoor());
            return;
        }
    }

    private IEnumerator OpenDoor()
    {
        float duration = 3f;
        float t = 0f;

        Quaternion startRot = doorObj.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 0, 90f); // 90도

        // ★ 문 열리는 소리 재생
        if (openSound != null)
            audioSource.PlayOneShot(openSound);

        while (t < duration)
        {
            doorObj.transform.rotation = Quaternion.Slerp(startRot, endRot, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        doorObj.transform.rotation = endRot;
    }
}
