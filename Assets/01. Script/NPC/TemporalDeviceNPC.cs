using UnityEngine;
using UnityEngine.UI;
using TMPro;

// NPC�� ��ȣ�ۿ��Ͽ� �ð� ��ġ UI�� ���� ���� Ŭ����
public class TemporalDeviceNPC : MonoBehaviour
{
    [Header("��ȣ�ۿ� ����")]
    [SerializeField] private float interactionRange = 3f; // ��ȣ�ۿ� ���� ����
    [SerializeField] private KeyCode interactionKey = KeyCode.F; // ��ȣ�ۿ� Ű

    [Header("UI ���")]
    [SerializeField] private GameObject interactionPrompt; // "F Ű�� ���� ��ȭ�ϱ�" ������Ʈ
    [SerializeField] private TextMeshProUGUI promptText; // ������Ʈ �ؽ�Ʈ
    [SerializeField] private GameObject temporalDeviceUI; // �ð� ��ġ UI �г�

    private Transform playerTransform; // �÷��̾� ��ġ
    private bool isPlayerInRange = false; // �÷��̾ ���� ���� �ִ���
    private bool isUIOpen = false; // UI�� �����ִ���

    private void Start()
    {
        // �÷��̾� ã��
        playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;

        if (playerTransform == null)
        {
            Debug.LogError("�÷��̾ ã�� �� �����ϴ�. Player �±װ� �ִ��� Ȯ���ϼ���.");
        }

        // ��ȣ�ۿ� ������Ʈ �ʱ� ���� ����
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // ������Ʈ �ؽ�Ʈ ����
        if (promptText != null)
        {
            promptText.text = $"{interactionKey} Ű�� ���� �ð� ��ġ ����";
        }

        // �ð� ��ġ UI �ʱ� ���� ����
        if (temporalDeviceUI != null)
        {
            temporalDeviceUI.SetActive(false);
        }
    }

    private void Update()
    {
        CheckPlayerDistance();

        // �÷��̾ ���� ���� �ְ� ��ȣ�ۿ� Ű�� ������ ��
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            ToggleTemporalDeviceUI();
        }

        // UI�� �����ִ� ���¿��� ESC Ű�� ������ �ݱ�
        if (isUIOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTemporalDeviceUI();
        }
    }

    // �÷��̾���� �Ÿ� üũ
    private void CheckPlayerDistance()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= interactionRange;

        // ���� ����/��Ż �� ������Ʈ ǥ��/����
        if (isPlayerInRange != wasInRange)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(isPlayerInRange);
            }
        }
    }

    // �ð� ��ġ UI ���
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

    // �ð� ��ġ UI ����
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
        // ��ȣ�ۿ� ������Ʈ �����
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // �ʿ��� ��� �÷��̾� ������ ����
        // PlayerController.instance.EnableMovement(false);
    }

    // �ð� ��ġ UI �ݱ�
    private void CloseTemporalDeviceUI()
    {
        if (temporalDeviceUI == null) return;

        temporalDeviceUI.SetActive(false);
        isUIOpen = false;

        // �÷��̾ ���� ���� ���� ������ ������Ʈ �ٽ� ǥ��
        if (isPlayerInRange && interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }

        // �÷��̾� ������ �ٽ� Ȱ��ȭ
        // PlayerController.instance.EnableMovement(true);
    }

    // ������ ��ȣ�ۿ� ���� �ð�ȭ (�����Ϳ����� ����)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}