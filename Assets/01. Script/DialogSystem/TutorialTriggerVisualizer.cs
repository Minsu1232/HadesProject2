using UnityEngine;

public class TutorialTriggerVisualizer : MonoBehaviour
{
    [Header("시각적 요소")]
    [SerializeField] private GameObject visualIndicatorPrefab; // 바닥에 표시될 이펙트
    [SerializeField] private float indicatorHeight = 0.1f; // 바닥 위 높이
    [SerializeField] private Color indicatorColor = new Color(0.2f, 0.8f, 0.2f, 0.4f); // 반투명 녹색

    [Header("활성화 조건")]
    [SerializeField] private string requiredFlag = ""; // 이 플래그가 있을 때만 표시 (비어있으면 항상 표시)
    [SerializeField] private string hideAfterFlag = ""; // 이 플래그가 있으면 숨김

    private GameObject indicatorInstance;
    private bool isActive = false;

    private void Start()
    {
        CheckConditionAndShow();

        // 다이얼로그 이벤트 구독 (플래그 변경 감지용)
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent += OnDialogEvent;
        }
    }

    private void OnDestroy()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent -= OnDialogEvent;
        }
    }

    // 다이얼로그 이벤트 처리 (플래그 변경 감지)
    private void OnDialogEvent(string eventName)
    {
        // 플래그 변경 이벤트인지 확인
        if (eventName.StartsWith("SetFlag:"))
        {
            CheckConditionAndShow();
        }
    }

    // 트리거에 들어왔을 때 실행할 메서드
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isActive)
        {
            // 콜라이더를 통과하면 시각적 표시 제거
            HideIndicator();

            // 만약 hideAfterFlag가 있으면 플래그 설정
            if (!string.IsNullOrEmpty(hideAfterFlag))
            {
                GameProgressManager.Instance?.SetFlag(hideAfterFlag, true);
            }
        }
    }

    // 조건을 확인하고 시각적 요소 표시 여부 결정
    private void CheckConditionAndShow()
    {
        bool shouldShow = true;

        // requiredFlag 확인 (있으면 플래그가 활성화되어야 표시)
        if (!string.IsNullOrEmpty(requiredFlag))
        {
            shouldShow = GameProgressManager.Instance?.GetFlag(requiredFlag) ?? false;
        }

        // hideAfterFlag 확인 (있으면 플래그가 활성화되었을 때 숨김)
        if (!string.IsNullOrEmpty(hideAfterFlag))
        {
            shouldShow = shouldShow && !(GameProgressManager.Instance?.GetFlag(hideAfterFlag) ?? false);
        }

        // 상태 변경 시에만 처리
        if (shouldShow != isActive)
        {
            isActive = shouldShow;

            if (isActive)
            {
                ShowIndicator();
            }
            else
            {
                HideIndicator();
            }
        }
    }

    // 시각적 표시 생성
    private void ShowIndicator()
    {
        if (visualIndicatorPrefab != null && indicatorInstance == null)
        {
            // 콜라이더 크기 가져오기
            Collider col = GetComponent<Collider>();
            Vector3 size = col != null ? col.bounds.size : new Vector3(2, 0.1f, 2);
            Vector3 position = transform.position + new Vector3(0, indicatorHeight, 0);

            // 바닥에 표시될 이펙트 생성
            indicatorInstance = Instantiate(visualIndicatorPrefab, position, Quaternion.identity);
            indicatorInstance.transform.parent = transform;

            // 크기 조정 (콜라이더 크기에 맞게)
            indicatorInstance.transform.localScale = new Vector3(size.x, 1, size.z);

            // 색상 설정
            Renderer renderer = indicatorInstance.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = indicatorColor;
            }
        }
    }

    // 시각적 표시 제거
    private void HideIndicator()
    {
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
            indicatorInstance = null;
        }
    }
}