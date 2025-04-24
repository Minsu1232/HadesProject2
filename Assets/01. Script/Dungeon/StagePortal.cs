using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StagePortal : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] public GameObject interactionPrompt;
    [SerializeField] private Image interactionProgressBar;

    [Header("포탈 설정")]
    [SerializeField] private float interactionDuration = 1.5f;
    [SerializeField] private GameObject portalUseEffectPrefab;
    public string targetStageID;

    private bool playerInRange = false;
    private bool isInteracting = false;
    private Coroutine interactionCoroutine;

    private void Start()
    {
      
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (interactionProgressBar != null) interactionProgressBar.fillAmount = 0f;
    }

    private void Update()
    {
        if (playerInRange)
        {
            // F키 누르고 있으면 상호작용 진행
            if (Input.GetKey(KeyCode.F) && !isInteracting)
            {
                StartInteraction();
            }
            // F키를 떼면 상호작용 취소
            else if (Input.GetKeyUp(KeyCode.F) && isInteracting)
            {
                CancelInteraction();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            if (isInteracting)
            {
                CancelInteraction();
            }
        }
    }

    // 포탈 상호작용 시작
    private void StartInteraction()
    {
        isInteracting = true;
        interactionCoroutine = StartCoroutine(InteractionProgress());
    }

    // 포탈 상호작용 취소
    private void CancelInteraction()
    {
        if (interactionCoroutine != null)
        {
            StopCoroutine(interactionCoroutine);
        }

        isInteracting = false;
        if (interactionProgressBar != null)
        {
            interactionProgressBar.fillAmount = 0f;
        }
    }

    // 상호작용 진행 바 처리
    private IEnumerator InteractionProgress()
    {
        float interactionTimer = 0f;

        while (interactionTimer < interactionDuration)
        {
            interactionTimer += Time.deltaTime;

            // 진행 바 업데이트
            if (interactionProgressBar != null)
            {
                interactionProgressBar.fillAmount = interactionTimer / interactionDuration;
            }

            yield return null;
        }

        // 상호작용 완료
        isInteracting = false;
        if (interactionProgressBar != null)
        {
            interactionProgressBar.fillAmount = 0f;
        }

        // 포탈 사용 이펙트
        if (portalUseEffectPrefab != null)
        {
            Instantiate(portalUseEffectPrefab, transform.position, Quaternion.identity);
        }

        // 던전 매니저를 통해 씬 전환
        UsePortal();
    }

    // 포탈 사용
    private void UsePortal()
    {
        if (string.IsNullOrEmpty(targetStageID))
        {
            Debug.LogError("포탈의 타겟 스테이지 ID가 설정되지 않았습니다.");
            return;
        }
        interactionPrompt.SetActive(false);
        // 트랜지션 효과
        if (SceneTransitionManager.Instance != null)
        {
            // 페이드 인 (어두워짐)
            SceneTransitionManager.Instance.FadeIn(() => {
                // 페이드 완료 후 다음 스테이지 로드
                DungeonManager.Instance.LoadStage(targetStageID);

                // 로딩이 완료되면 페이드 아웃 (밝아짐)
                StartCoroutine(DelayedFadeOut());
            });
        }
        else
        {
            // 트랜지션 매니저가 없는 경우 직접 로드
            DungeonManager.Instance.LoadStage(targetStageID);
        }
    }

    // 페이드 아웃 지연 처리
    private IEnumerator DelayedFadeOut()
    {
        // 약간의 지연 후 페이드 아웃
        yield return new WaitForSeconds(1f);

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.FadeOut();
        }
    }
}