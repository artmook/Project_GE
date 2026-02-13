using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class UInVideoManager : MonoBehaviour
{
    public enum GameState { Title, Cutscene, Playing, Ending }
    public GameState currentState = GameState.Title;

    [Header("Title UI")]
    public GameObject titleUI;

    [Header("Cutscene")]
    public GameObject cutsceneUI;
    public VideoPlayer cutscenePlayer;

    [Header("Player")]
    public PlayerController player;

    [Header("UI Elements")]
    public GameObject camcorderUI;

    // 사망 카운트 시스템
    [Header("Death Count System")]
    public int deathCount = 0;
    public int deathLimit = 3;

    // 엔딩 영상 재생용 UI
    [Header("Ending Cutscene")]
    public GameObject endingUI;
    public VideoPlayer endingPlayer;

    private bool canSkipCutscene = false;

    // 엔딩 영상 재생 중 여부 체크용
    private bool isEndingPlaying = false;

    void Start()
    {
        if (!GameStateManager.Instance.titleShown)
        {
            currentState = GameState.Title;

            if (titleUI != null) titleUI.SetActive(true);
            if (cutsceneUI != null) cutsceneUI.SetActive(false);
            if (endingUI != null) endingUI.SetActive(false);

            if (player != null) player.enabled = false;
            if (camcorderUI != null) camcorderUI.SetActive(false);

            GameStateManager.Instance.titleShown = true;

            EntityAI.isAnyMonsterAttacking = true;

            if (UIManager.Instance != null) UIManager.Instance.SetCrosshair(false);
        }
        else
        {
            StartGameplay();
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case GameState.Title:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartCutscene();
                }
                break;

            case GameState.Cutscene:
                if (canSkipCutscene && Input.GetKeyDown(KeyCode.Return))
                {
                    SkipCutscene();
                }
                break;

            case GameState.Ending:

                // 엔딩 영상 스킵 기능 추가
                if (isEndingPlaying && Input.GetKeyDown(KeyCode.Return))
                {
                    EndEndingCutscene();
                    break;
                }

                // 엔딩 영상 종료 감지
                if (isEndingPlaying && endingPlayer != null)
                {
                    if (!endingPlayer.isPlaying && endingPlayer.time > 0.1f)
                    {
                        EndEndingCutscene();
                    }
                }
                break;
        }
    }


    void StartCutscene()
    {
        currentState = GameState.Cutscene;

        if (titleUI != null) titleUI.SetActive(false);
        if (cutsceneUI != null) cutsceneUI.SetActive(true);
        if (endingUI != null) endingUI.SetActive(false);

        if (UIManager.Instance != null) UIManager.Instance.SetCrosshair(false);

        EntityAI.isAnyMonsterAttacking = true;

        if (cutscenePlayer != null)
        {
            cutscenePlayer.Play();
            cutscenePlayer.loopPointReached += OnCutsceneEnd;
        }

        Invoke(nameof(EnableCutsceneSkip), 0.2f);
    }

    void EnableCutsceneSkip()
    {
        canSkipCutscene = true;
    }

    void SkipCutscene()
    {
        if (cutscenePlayer != null)
        {
            cutscenePlayer.loopPointReached -= OnCutsceneEnd;
            cutscenePlayer.Stop();
        }

        StartGameplay();
    }

    void OnCutsceneEnd(VideoPlayer vp)
    {
        cutscenePlayer.loopPointReached -= OnCutsceneEnd;
        StartGameplay();
    }

    void StartGameplay()
    {
        currentState = GameState.Playing;

        if (cutsceneUI != null) cutsceneUI.SetActive(false);
        if (endingUI != null) endingUI.SetActive(false);

        if (player != null) player.enabled = true;
        if (camcorderUI != null) camcorderUI.SetActive(true);

        if (UIManager.Instance != null) UIManager.Instance.SetCrosshair(true);

        canSkipCutscene = false;
        EntityAI.isAnyMonsterAttacking = false;
    }

    // 새로운 기능: 사망 시 호출되는 함수
    public void RegisterDeath()
    {
        deathCount++;

        // 사망 횟수 초과 시 엔딩 영상 실행
        if (deathCount >= deathLimit)
        {
            deathCount = 0;
            StartCoroutine(HandleEndingAfterReset());
        }
    }

    // ResetGameRoutine 이후 일정 시간 대기 후 엔딩 영상 재생
    IEnumerator HandleEndingAfterReset()
    {
        // ResetGameRoutine이 종료되어 Title 상태로 돌아갈 시간을 확보
        yield return new WaitForSeconds(0.1f);

        PlayEndingCutscene();
    }

    // 엔딩 영상 시작 함수
    public void PlayEndingCutscene()
    {
        currentState = GameState.Ending;

        if (player != null) player.enabled = false;
        if (camcorderUI != null) camcorderUI.SetActive(false);

        EntityAI.isAnyMonsterAttacking = true;

        if (titleUI != null) titleUI.SetActive(false);
        if (cutsceneUI != null) cutsceneUI.SetActive(false);
        if (endingUI != null) endingUI.SetActive(true);

        if (UIManager.Instance != null) UIManager.Instance.SetCrosshair(false);

        // 엔딩 영상 재생 상태 설정
        isEndingPlaying = true;

        if (endingPlayer != null)
        {
            endingPlayer.time = 0;
            endingPlayer.Play();
        }
    }

    // 엔딩 영상 종료 처리 함수
    void EndEndingCutscene()
    {
        isEndingPlaying = false;

        // 엔딩 영상 강제 중단
        if (endingPlayer != null)
        {
            endingPlayer.Stop();          // 영상과 소리 즉시 중단
            endingPlayer.time = 0;        // 재생 위치 초기화
                                          // loopPointReached 이벤트 사용하지 않으므로 제거 코드 필요 없음
        }

        if (endingUI != null) endingUI.SetActive(false);

        if (titleUI != null) titleUI.SetActive(true);

        currentState = GameState.Title;

        EntityAI.isAnyMonsterAttacking = true;

        if (player != null) player.enabled = false;
        if (camcorderUI != null) camcorderUI.SetActive(false);

        if (UIManager.Instance != null) UIManager.Instance.SetCrosshair(false);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
