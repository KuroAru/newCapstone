using UnityEngine;
using System.Collections; // 코루틴(IEnumerator) 사용을 위해 필수

public class AudioController : MonoBehaviour
{
    // --- 기존 변수들 ---
    [Header("BGM Settings")]
    public AudioClip musicToPlay;
    private AudioSource audioSource;

    // --- 발자국 소리를 위한 변수들 ---
    [Header("SFX Settings")]
    public AudioClip footstepSound; // 인스펙터에서 발자국 소리 파일을 연결할 변수
    public float delayBetweenSteps = 0.3f; // 발자국 소리 사이의 간격 (초 단위)

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // --- 음악 재생 함수 ---
    public void PlayMusic()
    {
        if (audioSource != null && musicToPlay != null)
        {
            audioSource.clip = musicToPlay;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    // --- [새로 추가됨] 음악 정지 함수 ---
    /// <summary>
    /// 재생 중인 음악을 즉시 멈추는 함수입니다.
    /// Fungus에서 Call Method로 이 함수를 호출하면 소리가 꺼집니다.
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    // --- 발자국 소리 재생 함수 ---
    /// <summary>
    /// 발자국 소리를 4회 반복해서 재생시키는 함수 (이 함수를 호출!)
    /// </summary>
    public void PlayFootstep()
    {
        // footstepSound 오디오 클립이 할당되어 있을 때만 코루틴을 실행합니다.
        if (footstepSound != null)
        {
            StartCoroutine(FootstepCoroutine());
        }
        else
        {
            Debug.LogWarning("Footstep Sound가 인스펙터에 할당되지 않았습니다!");
        }
    }

    // --- 발자국 소리 코루틴 ---
    /// <summary>
    /// 실제 소리 재생과 지연을 처리하는 코루틴
    /// </summary>
    private IEnumerator FootstepCoroutine()
    {
        // for 반복문을 사용해 4번 반복합니다.
        for (int i = 0; i < 4; i++)
        {
            // 발자국 소리를 한 번 재생합니다.
            // PlayOneShot은 배경음악이 재생 중이어도 끊기지 않고 위에 겹쳐서 소리가 납니다.
            audioSource.PlayOneShot(footstepSound);

            // delayBetweenSteps에 설정된 시간만큼 기다립니다.
            yield return new WaitForSeconds(delayBetweenSteps);
        }
    }
}