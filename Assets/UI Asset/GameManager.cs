using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int deathCount = 0;
    public int deathLimit = 3;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 게임 전체 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterDeath()
    {
        deathCount++;

        if (deathCount >= deathLimit)
        {
            deathCount = 0; // 초기화 (선택)
            PlayDeathEndingVideo();
        }
    }

    void PlayDeathEndingVideo()
    {
        // 영상 전용 씬으로 이동
        SceneManager.LoadScene("DeathVideoScene");
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
