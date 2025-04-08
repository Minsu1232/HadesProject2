using UnityEngine;

public class BossEncounterTrigger : MonoBehaviour
{
    [SerializeField] private string bossEncounterDialogID; // ���� ���� ��ȭ ID
    [SerializeField] private bool triggerOnce = true; // �� ���� Ʈ����
    [SerializeField] private Transform bossTransform; // ���� ��ġ
    [SerializeField] private string bossFightEventName = "StartBossFight"; // ���� ���� �̺�Ʈ �̸�

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
            // ī�޶� ������ ���ϵ��� �̺�Ʈ �߻�
            if (DialogSystem.Instance != null)
            {
                DialogSystem.Instance.StartDialog(bossEncounterDialogID);
                hasTriggered = true;
                GameProgressManager.Instance.MarkDialogAsShown(bossEncounterDialogID);
                // ���� ���� �̺�Ʈ�� ���̾�α� ������ Ʈ������ ���̹Ƿ� ���⼭�� �ʿ� ����
            }
        }
    }

    // ���� ���� ���� (���̾�α� �̺�Ʈ���� ȣ��)
    public void StartBossFight()
    {
        // ���� AI Ȱ��ȭ �� �ʿ��� �۾� ����
        // ��: GetComponent<BossAI>().StartFight();
        Debug.Log("���� ���� ����!");
    }

    private void OnEnable()
    {
        // ���̾�α� �̺�Ʈ ������ ���
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

    // ���̾�α� �̺�Ʈ ó��
    private void HandleDialogEvent(string eventName)
    {
        if (eventName == bossFightEventName)
        {
            StartBossFight();
        }
    }
}