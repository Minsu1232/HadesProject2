using UnityEngine;

public class DungeonEntranceTrigger : MonoBehaviour
{
    [SerializeField] private string dungeonEntranceDialogID; // 던전 입장 대화 ID
    [SerializeField] private bool triggerOnce = true; // 한 번만 트리거
    [SerializeField] private bool checkIfAlreadyShown = true; // 이미 본 대화인지 체크
    
    private bool hasTriggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!triggerOnce || !hasTriggered))
        {
            // 이미 본 대화인지 확인 (필요시)
            if (checkIfAlreadyShown && GameProgressManager.Instance != null && 
                GameProgressManager.Instance.IsDialogShown(dungeonEntranceDialogID))
            {
                return;
            }
            
            GameProgressManager.Instance.MarkDialogAsShown(dungeonEntranceDialogID);
            // 대화 시작
            if (DialogSystem.Instance != null)
            {
                DialogSystem.Instance.StartDialog(dungeonEntranceDialogID);
                hasTriggered = true;
            }
        }
    }
}