using UnityEngine;
using System.Collections;

public class EffectDoor : MonoBehaviour, IInteractable
{
    [Header("상호작용 텍스트")]
    public string interactString = "Door Open(F)";
    public string afterTryString = "It's Locked";

    [Header("문 오브젝트 설정")]
    public GameObject doorObj;

    [Header("사운드 설정")]
    public AudioClip openSound;
    public AudioClip laughSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;

    private AudioSource audioSource;

    private bool isAnimating = false;   //문이 열리고 닫히는 동안 true

    [HideInInspector]
    public string uniqueID;

    private void Awake()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string posStr = transform.position.ToString("F1");
        uniqueID = $"{sceneName}_{posStr}";
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// 플레이어가 보는 상호작용 텍스트
    /// </summary>
    public string GetInteractPrompt()
    {
        // ★ 문이 열리거나 닫히는 중에는 아무 문자열도 표시하지 않기
        if (isAnimating)
            return "";

        // 이미 시도한 문 → "Locked Door"
        if (GameStateManager.Instance.IsDoorTried(uniqueID))
            return afterTryString;

        // 시도 전 → "Door Open(F)"
        return interactString;
    }

    public void Interact()
    {
        // 애니메이션 중이면 상호작용 무시
        if (isAnimating)
            return;

        // 아직 시도하지 않았을 때 → 문 열고 닫는 연출 실행
        if (!GameStateManager.Instance.IsDoorTried(uniqueID))
        {
            StartCoroutine(FakeOpenClose());
            return;
        }

        // 이미 시도한 문 → 잠긴 소리만 재생
        if (lockedSound != null)
            audioSource.PlayOneShot(lockedSound);
    }

    private IEnumerator FakeOpenClose()
    {
        isAnimating = true;  // ★ 문자열 표시 차단 시작

        float openDuration = 3f;
        float waitDuration = 1.3f;
        float closeDuration = 0.5f;

        Quaternion startRot = doorObj.transform.rotation;
        Quaternion openRot = startRot * Quaternion.Euler(0, 0, 30f);

        // 열리는 소리
        if (openSound != null)
            audioSource.PlayOneShot(openSound);

        // 1) 문 열기
        float t = 0;
        while (t < openDuration)
        {
            doorObj.transform.rotation =
                Quaternion.Slerp(startRot, openRot, t / openDuration);
            t += Time.deltaTime;
            yield return null;
        }
        doorObj.transform.rotation = openRot;

        // 웃음소리
        if (laughSound != null)
            audioSource.PlayOneShot(laughSound);

        // 2) 대기
        yield return new WaitForSeconds(waitDuration);

        // 닫히는 소리
        if (closeSound != null)
            audioSource.PlayOneShot(closeSound);

        // 3) 문 닫기
        t = 0;
        while (t < closeDuration)
        {
            doorObj.transform.rotation =
                Quaternion.Slerp(openRot, startRot, t / closeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        doorObj.transform.rotation = startRot;

        //문이 완전히 닫힌 후 시도 완료로 기록
        GameStateManager.Instance.AddTriedDoor(uniqueID);

        isAnimating = false; //문자열 표시 재허용
    }
}
