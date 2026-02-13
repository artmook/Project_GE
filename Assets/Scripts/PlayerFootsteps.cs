using UnityEngine;

// CharacterController와 AudioSource 필수
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    // ============================================================
    // Inspector Variables
    // ============================================================

    // 발자국 소리 목록 (랜덤 재생)
    public AudioClip[] footstepSounds;

    // 걷기/달리기 발소리 간격
    public float timeBetweenSteps = 0.5f;
    public float timeBetweenStepsWhenRun;

    // ============================================================
    // Internal Variables
    // ============================================================
    private CharacterController controller;
    private AudioSource audioSource;

    private float stepTimer;      // 다음 발소리까지 남은 시간
    private bool isMuted = false; // Silent Step 기능용 음소거 플래그
    private bool isDead = false;  // [추가됨] 죽었는지 체크하는 플래그

    // ============================================================
    // 초기화
    // ============================================================
    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }


    // ============================================================
    // 매 프레임 업데이트
    // ============================================================
    void Update()
    {
        // Silent Step 활성화 시 또는 죽었을 때 완전 음소거
        if (isMuted || isDead)
            return;

        // 플레이어가 땅에 있고, 속도가 충분하면 발소리 처리
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstepSound();

                // 달리기인지 걷기인지에 따라 간격 변경
                stepTimer = Input.GetMouseButton(1) ?
                            timeBetweenStepsWhenRun :
                            timeBetweenSteps;
            }
        }
        else
        {
            // 멈췄다가 다시 걸을 때 즉시 재생되지 않도록 소량 리셋
            stepTimer = 0.1f;
        }
    }


    // ============================================================
    // 실제 발소리 재생
    // ============================================================
    void PlayFootstepSound()
    {
        if (isMuted || isDead) return;
        if (footstepSounds.Length == 0) return;

        int index = Random.Range(0, footstepSounds.Length);

        // 피치 랜덤 변화를 줘서 자연스럽게 함
        audioSource.pitch = Random.Range(0.9f, 1.1f);

        audioSource.PlayOneShot(footstepSounds[index]);
    }


    // ============================================================
    // Public API
    // ============================================================
    public void SetMuted(bool mute)
    {
        isMuted = mute;
    }

    // ★ [추가된 기능] 데스씬 시작 시 이 함수를 호출하세요!
    public void StopFootsteps()
    {
        isDead = true;          // 더 이상 Update에서 발소리 계산 안 함
        audioSource.Stop();     // 현재 재생 중인 소리(PlayOneShot) 즉시 끊음
        enabled = false;        // (선택 사항) 스크립트 자체를 비활성화해서 확실하게 처리
    }
}