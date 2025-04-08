using UnityEngine;
using TMPro;

public class InteractableWeapon : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField] private string weaponName = "GreatSword"; // ������ ���� �̸� (WeaponFactory�� ��ϵ� �̸�)
    [SerializeField] private string interactionPrompt = "FŰ�� ���� ���� �����ϱ�";
    [SerializeField] private float interactionRange = 3f; // ��ȣ�ۿ� ����

    [Header("UI ���")]
    [SerializeField] private GameObject promptUI; // ������Ʈ UI ���� ������Ʈ
    [SerializeField] private TextMeshProUGUI promptText; // ������Ʈ �ؽ�Ʈ

    private Transform playerTransform;
    private bool playerInRange = false;
    private bool promptShown = false;
    private WeaponService weaponService;

    // ���� ���� �̺�Ʈ
    public event System.Action OnWeaponEquipped;

    private void Start()
    {
        // �÷��̾� ã��
        playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;
        if (playerTransform == null)
        {
            Debug.LogError("�÷��̾ ã�� �� �����ϴ�.");
        }

        // WeaponService ���� ��������
        weaponService = GameInitializer.Instance.GetWeaponService();
        if (weaponService == null)
        {
            Debug.LogError("WeaponService�� ã�� �� �����ϴ�.");
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
            EquipWeapon();
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

    private async void EquipWeapon()
    {
        if (weaponService == null) return;

        // ���� ���� �õ�
        bool success = await weaponService.EquipWeapon(weaponName);

        if (success)
        {
            Debug.Log($"{weaponName} ���� ����!");

            // �̺�Ʈ �߻�
            OnWeaponEquipped?.Invoke();
        }
        else
        {
            Debug.LogError($"{weaponName} ���� ����!");
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

        Debug.Log($"{interactionPrompt} ({weaponName})");
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
        Gizmos.color = Color.blue; // ����� �Ķ������� ǥ��
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}