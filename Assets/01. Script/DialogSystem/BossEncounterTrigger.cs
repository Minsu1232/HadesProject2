using UnityEngine;

public class BossEncounterTrigger : MonoBehaviour
{
    [SerializeField] private string bossEncounterDialogID; // 보스 조우 대화 ID
    [SerializeField] private bool triggerOnce = true; // 한 번만 트리거
    [SerializeField] private Transform bossTransform; // 보스 위치
    [SerializeField] private string bossFightEventName = "StartBossFight"; // 전투 시작 이벤트 이름

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!triggerOnce || !hasTriggered))
        {
            if (GameProgressManager.Instance != null &&
                GameProgressManager.Instance.IsDialogShown(bossEncounterDialogID))
            {
                
                return;
            }
            // 카메라가 보스를 향하도록 이벤트 발생
            if (DialogSystem.Instance != null)
            {
                DialogSystem.Instance.StartDialog(bossEncounterDialogID);
                hasTriggered = true;
                GameProgressManager.Instance.MarkDialogAsShown(bossEncounterDialogID);
                // 전투 시작 이벤트를 다이얼로그 내에서 트리거할 것이므로 여기서는 필요 없음
            }
        }
    }

    // 보스 전투 시작 (다이얼로그 이벤트에서 호출)
    public void StartBossFight()
    {
        // 보스 AI 활성화 등 필요한 작업 수행
        // 예: GetComponent<BossAI>().StartFight();
        Debug.Log("보스 전투 시작!");
    }

    private void OnEnable()
    {
        // 다이얼로그 이벤트 리스너 등록
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent += HandleDialogEvent;
        }
    }

    private void OnDisable()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent -= HandleDialogEvent;
        }
    }

    // 다이얼로그 이벤트 처리
    private void HandleDialogEvent(string eventName)
    {
        if (eventName == bossFightEventName)
        {
            StartBossFight();
        }
    }
}