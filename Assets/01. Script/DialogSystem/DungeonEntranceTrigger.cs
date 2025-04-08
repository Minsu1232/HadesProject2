using UnityEngine;

public class DungeonEntranceTrigger : MonoBehaviour
{
    [SerializeField] private string dungeonEntranceDialogID; // ���� ���� ��ȭ ID
    [SerializeField] private bool triggerOnce = true; // �� ���� Ʈ����
    [SerializeField] private bool checkIfAlreadyShown = true; // �̹� �� ��ȭ���� üũ
    
    private bool hasTriggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!triggerOnce || !hasTriggered))
        {
            // �̹� �� ��ȭ���� Ȯ�� (�ʿ��)
            if (checkIfAlreadyShown && GameProgressManager.Instance != null && 
                GameProgressManager.Instance.IsDialogShown(dungeonEntranceDialogID))
            {
                return;
            }
            
            GameProgressManager.Instance.MarkDialogAsShown(dungeonEntranceDialogID);
            // ��ȭ ����
            if (DialogSystem.Instance != null)
            {
                DialogSystem.Instance.StartDialog(dungeonEntranceDialogID);
                hasTriggered = true;
            }
        }
    }
}