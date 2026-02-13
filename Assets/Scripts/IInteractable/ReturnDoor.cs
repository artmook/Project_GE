using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReturnDoor : MonoBehaviour, IInteractable
{
    public string password = "1234";
    public string interactString = "Door Open(F)";
    public string sceneToMove;
    public string sceneToEnd;
    public Transform doorMesh;
    public KeypadUI keypadUI;
    [Header("연출 오브젝트 연결")]
    public GameObject successPlane;
    public GameObject failPlane;

    [Header("연출 설정")]
    public float openDuration = 3.0f;
    public float waitBeforeLoad = 1.0f;
    public AudioClip openSound;
    private AudioSource audioSource;
    private bool isInteracting = false;
    void Start(){
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        if (successPlane != null) successPlane.SetActive(false);
        if (failPlane != null) failPlane.SetActive(false);
    }

    public string GetInteractPrompt()
    {
        if (isInteracting) return "";
        return interactString;
    }

    public void Interact()
    {
        if (isInteracting) return;
        isInteracting = true;
        keypadUI.OpenKeypad(password, OnCorrectPassword, OnFailPassword, OnCancelPassword);
    }

    void OnCorrectPassword(){
        StartCoroutine(EndingSequenceRoutine(true));
        isInteracting = false;
    }
    void OnFailPassword(){
        StartCoroutine(EndingSequenceRoutine(false));
        isInteracting = false;
    }
    void OnCancelPassword(){
        isInteracting = false;
    }

    IEnumerator EndingSequenceRoutine(bool isSuccess)
    {
        // 1. 결과에 맞는 플레인 활성화
        if (isSuccess)
        {
            if (successPlane != null) successPlane.SetActive(true);
            if (failPlane != null) failPlane.SetActive(false);
        }
        else
        {
            if (successPlane != null) successPlane.SetActive(false);
            if (failPlane != null) failPlane.SetActive(true);
        }

        // 2. 문 열기 (사운드 & 회전)
        if (openSound != null) audioSource.PlayOneShot(openSound);

        Quaternion startRot = doorMesh.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 0, -90); // 90도 오픈

        float time = 0;
        while (time < 1f)
        {
            time += Time.deltaTime / openDuration;
            doorMesh.localRotation = Quaternion.Slerp(startRot, endRot, time);
            yield return null;
        }

        // 3. 문이 활짝 열린 상태로 잠시 대기 (플레이어가 안을 볼 시간)
        yield return new WaitForSeconds(waitBeforeLoad);

        if (isSuccess)
        {
            SceneManager.LoadScene(sceneToEnd);
        }
        else
        {
            GameStateManager.Instance.targetID=GameStateManager.Instance.doorIdToReturn;
            SceneManager.LoadScene(sceneToMove);
        }
    }
}
