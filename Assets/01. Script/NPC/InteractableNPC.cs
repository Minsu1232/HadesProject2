// InteractableNPC.cs - ��ȣ�ۿ� ������ NPC
using UnityEngine;
using TMPro;

public class InteractableNPC : MonoBehaviour
{
    [Header("�⺻ ����")]
    [SerializeField] private string npcName = "��������";
    [SerializeField] private string interactionPrompt = "FŰ�� ���� ��ȭ�ϱ�";
    [SerializeField] private float interactionRange = 3f; // ��ȣ�ۿ� ���� �߰�
    [SerializeField] private VillageInteractionManager interactionManager;
    [SerializeField] private NPCType npcType = NPCType.StatUpgrade;

    [Header("UI ���")]
    [SerializeField] private GameObject promptUI; // ������Ʈ UI ���� ������Ʈ
    [SerializeField] private TextMeshProUGUI promptText; // ������Ʈ �ؽ�Ʈ (���� ����)

    private Transform playerTransform; // �÷��̾� Ʈ������ �߰�
    private bool playerInRange = false;
    private bool promptShown = false;

    public enum NPCType
    {
        StatUpgrade,
        Shop,
        Quest
    }

    private void Start()
    {
        // �÷��̾� ã��
        playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;
        if (playerTransform == null)
        {
            Debug.LogError("�÷��̾ ã�� �� �����ϴ�.");
        }

        // ���� �� ������Ʈ �����
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

  
    }

    private void Update()
    {
        // �÷��̾���� �Ÿ� üũ
        CheckPlayerDistance();

        // �÷��̾ ���� ���� �ְ� F Ű�� ������ �� ��ȣ�ۿ�
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            Interact();
        }
    }

    // �÷��̾���� �Ÿ� üũ
    private void CheckPlayerDistance()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        // ���� ����/��Ż �� ������Ʈ ǥ��/����
        if (playerInRange != wasInRange)
        {
            if (playerInRange)
            {
                ShowInteractionPrompt();
            }
            else
            {
                HideInteractionPrompt();
            }
        }
    }

    private void Interact()
    {
        if (interactionManager == null)
        {
            interactionManager = FindObjectOfType<VillageInteractionManager>();
            if (interactionManager == null)
            {
                Debug.LogError("VillageInteractionManager�� ã�� �� �����ϴ�.");
                return;
            }
        }

        switch (npcType)
        {
            case NPCType.StatUpgrade:
                interactionManager.OpenStatUpgradeShop();
                // ������Ʈ ����� (UI ���� ��)
                HideInteractionPrompt();
                break;
                // �ٸ� NPC Ÿ�Ե鿡 ���� ó���� �߰� ����
        }
    }

    private void ShowInteractionPrompt()
    {
        if (promptShown) return;
        // ������Ʈ �ؽ�Ʈ ���� (�ִ� ���)
        if (promptText != null)
        {
            promptText.text = $"{interactionPrompt}";
        }
        // UI�� ��ȣ�ۿ� �ȳ� ǥ��
        if (promptUI != null)
        {
            promptUI.SetActive(true);
        }

        Debug.Log($"{interactionPrompt} ({npcName})");
        promptShown = true;
    }

    private void HideInteractionPrompt()
    {
        if (!promptShown) return;

        // UI���� ��ȣ�ۿ� �ȳ� �����
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

        promptShown = false;
    }

    // �����Ϳ��� ��ȣ�ۿ� ���� �ð�ȭ
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}