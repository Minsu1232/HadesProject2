using UnityEngine;
using UnityEngine.UI;
using TMPro;

// NPC와 상호작용하여 시간 장치 UI를 열기 위한 클래스
public class TemporalDeviceNPC : MonoBehaviour
{
    [Header("상호작용 설정")]
    [SerializeField] private float interactionRange = 3f; // 상호작용 가능 범위
    [SerializeField] private KeyCode interactionKey = KeyCode.F; // 상호작용 키

    [Header("UI 요소")]
    [SerializeField] private GameObject interactionPrompt; // "F 키를 눌러 대화하기" 프롬프트
    [SerializeField] private TextMeshProUGUI promptText; // 프롬프트 텍스트
    [SerializeField] private GameObject temporalDeviceUI; // 시간 장치 UI 패널

    private Transform playerTransform; // 플레이어 위치
    private bool isPlayerInRange = false; // 플레이어가 범위 내에 있는지
    private bool isUIOpen = false; // UI가 열려있는지

    private void Start()
    {
        // 플레이어 찾기
        playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;

        if (playerTransform == null)
        {
            Debug.LogError("플레이어를 찾을 수 없습니다. Player 태그가 있는지 확인하세요.");
        }

        // 상호작용 프롬프트 초기 상태 설정
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // 프롬프트 텍스트 설정
        if (promptText != null)
        {
            promptText.text = $"{interactionKey} 키를 눌러 시간 장치 보기";
        }

        // 시간 장치 UI 초기 상태 설정
        if (temporalDeviceUI != null)
        {
            temporalDeviceUI.SetActive(false);
        }
    }

    private void Update()
    {
        CheckPlayerDistance();

        // 플레이어가 범위 내에 있고 상호작용 키를 눌렀을 때
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            ToggleTemporalDeviceUI();
        }

        // UI가 열려있는 상태에서 ESC 키를 누르면 닫기
        if (isUIOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTemporalDeviceUI();
        }
    }

    // 플레이어와의 거리 체크
    private void CheckPlayerDistance()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= interactionRange;

        // 범위 진입/이탈 시 프롬프트 표시/숨김
        if (isPlayerInRange != wasInRange)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(isPlayerInRange);
            }
        }
    }

    // 시간 장치 UI 토글
    private void ToggleTemporalDeviceUI()
    {
        if (isUIOpen)
        {
            CloseTemporalDeviceUI();
        }
        else
        {
            OpenTemporalDeviceUI();
        }
    }

    // 시간 장치 UI 열기
    private void OpenTemporalDeviceUI()
    {
        if (temporalDeviceUI == null) return;

        temporalDeviceUI.SetActive(true);
        isUIOpen = true;
        SimpleTemporalDeviceUI deviceUI = temporalDeviceUI.GetComponentInParent<SimpleTemporalDeviceUI>();
        if (deviceUI != null)
        {
            deviceUI.OpenUI();
        }
        // 상호작용 프롬프트 숨기기
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // 필요한 경우 플레이어 움직임 제한
        // PlayerController.instance.EnableMovement(false);
    }

    // 시간 장치 UI 닫기
    private void CloseTemporalDeviceUI()
    {
        if (temporalDeviceUI == null) return;

        temporalDeviceUI.SetActive(false);
        isUIOpen = false;

        // 플레이어가 아직 범위 내에 있으면 프롬프트 다시 표시
        if (isPlayerInRange && interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }

        // 플레이어 움직임 다시 활성화
        // PlayerController.instance.EnableMovement(true);
    }

    // 기즈모로 상호작용 범위 시각화 (에디터에서만 보임)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}