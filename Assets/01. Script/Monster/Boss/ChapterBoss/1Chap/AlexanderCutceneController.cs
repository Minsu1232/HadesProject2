using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class AlexanderCutsceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector timelineDirector;
    [SerializeField] private GameObject letterboxTop;
    [SerializeField] private GameObject letterboxBottom;
    [SerializeField] private CinemachineVirtualCamera mainGameplayCamera;

    private CameraFollow originalCameraScript;

    public void PlayCutscene()
    {
        // 게임플레이 카메라 비활성화
        if (mainGameplayCamera.gameObject.activeSelf)
        {
            originalCameraScript = Camera.main.GetComponent<CameraFollow>();
            if (originalCameraScript != null)
                originalCameraScript.enabled = false;

            mainGameplayCamera.gameObject.SetActive(false);
        }

        // 레터박스 활성화
        letterboxTop.SetActive(true);
        letterboxBottom.SetActive(true);

        // 타임라인 재생
        timelineDirector.Play();

        // 타임라인 완료 이벤트 리스너 추가
        timelineDirector.stopped += OnTimelineFinished;
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        // 이벤트 리스너 제거
        timelineDirector.stopped -= OnTimelineFinished;

        // 게임플레이 카메라 복구
        mainGameplayCamera.gameObject.SetActive(true);
        if (originalCameraScript != null)
            originalCameraScript.enabled = true;

        // 레터박스 비활성화
        letterboxTop.SetActive(false);
        letterboxBottom.SetActive(false);

        // 보스 페이즈 변경 신호 등 게임플레이 관련 처리
        BossAI bossAI = FindObjectOfType<BossAI>();
        if (bossAI != null)
        {
            // 다음 페이즈 시작 등의 로직
        }
    }
}