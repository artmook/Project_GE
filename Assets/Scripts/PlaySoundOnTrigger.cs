using UnityEngine;

public class PlaySoundOnTrigger : MonoBehaviour
{
    // 1. 유니티 에디터에서 재생할 Audio Source를 이 변수로 끌어다 놓습니다.
    public AudioSource audioToPlay;

    // 2. 소리가 한 번만 재생되도록 체크하는 변수
    private bool hasPlayed = false;

    // 3. (필수) 플레이어가 트리거 영역에 들어왔을 때 호출되는 함수
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("트리거 작동! 들어온 오브젝트: " + other.name);
        // 4. 만약 들어온 것이 'Player' 태그를 가진 오브젝트이고 (권장)
        //    그리고 아직 소리가 재생된 적이 없다면
        if (other.CompareTag("Player") && !hasPlayed)
        {
            Debug.Log("플레이어 확인! 오디오 재생 시도."); // 이것도 추가하면 좋습니다
            // 5. 오디오를 재생합니다.
            audioToPlay.Play();

            // 6. 재생되었다고 표시 (다시는 재생되지 않음)
            hasPlayed = true;
        }
    }
}