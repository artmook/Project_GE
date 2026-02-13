using UnityEngine;
using UnityEngine.Video;

public class EndingManager : MonoBehaviour
{
    [Header("Player")]
    public PlayerController player;
    // 엔딩 영상 재생용 UI
    [Header("Ending Cutscene")]
    public GameObject endingUI;
    public VideoPlayer endingPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        PlayEndingCutscene();
    }

    // 엔딩 영상 시작 함수
    public void PlayEndingCutscene()
    {
        if (player != null) player.enabled = false;

        if (endingUI != null) endingUI.SetActive(true);

        if (UIManager.Instance != null) UIManager.Instance.SetCrosshair(false);

        if (endingPlayer != null)
        {
            endingPlayer.time = 0;
            endingPlayer.Play();
        }
    }
}
