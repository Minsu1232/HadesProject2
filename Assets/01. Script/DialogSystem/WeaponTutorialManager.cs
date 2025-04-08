// WeaponTutorialManager.cs
using UnityEngine;
using UnityEngine.UI;

public class WeaponTutorialManager : MonoBehaviour
{
    [SerializeField] private string nextDialogID = "combat_tutorial";
    [SerializeField] private InteractableWeapon weaponInteractable;
    [SerializeField] private GameObject trainingDummy;
    [SerializeField] private float dummyHighlightDelay = 2f;

    private bool weaponEquipped = false;
    private bool tutorialComplete = false;

    private void Start()
    {
        // DialogSystem���� �̺�Ʈ ������
        DialogSystem.OnDialogEvent += HandleDialogEvent;

        // ���� ���� �̺�Ʈ ������ ���
        if (weaponInteractable != null)
        {
            weaponInteractable.OnWeaponEquipped += OnWeaponEquipped;
        }

        // ó������ ���� ���̶���Ʈ ��Ȱ��ȭ
        if (trainingDummy != null)
        {
            SetDummyHighlight(false);
        }
    }

    private void OnDestroy()
    {
        DialogSystem.OnDialogEvent -= HandleDialogEvent;

        if (weaponInteractable != null)
        {
            weaponInteractable.OnWeaponEquipped -= OnWeaponEquipped;
        }
    }

    // ���̾�α� �̺�Ʈ ó��
    private void HandleDialogEvent(string eventName)
    {
        if (eventName == "EnableMovement")
        {
            // ������ weapon_tutorial ��ȭ ����, ���� ���̶���Ʈ Ȱ��ȭ
            Invoke("HighlightDummy", dummyHighlightDelay);
        }
    }

    // ���� ���� �̺�Ʈ ó��
    private void OnWeaponEquipped()
    {
        Debug.Log("���Ⱑ �����Ǿ����ϴ�!");
        weaponEquipped = true;

        // ���� ���� Ȯ��
        CheckTutorialProgress();
    }

    // ���� ���̶���Ʈ ǥ��
    private void HighlightDummy()
    {
        if (trainingDummy != null && weaponEquipped)
        {
            SetDummyHighlight(true);
        }
    }

    // ���� ���̶���Ʈ ȿ�� ����
    private void SetDummyHighlight(bool active)
    {
        // ���̶���Ʈ ȿ�� (Outline ������Ʈ ��)�� Ȱ��ȭ/��Ȱ��ȭ
        Outline outlineComponent = trainingDummy.GetComponent<Outline>();
        if (outlineComponent != null)
        {
            outlineComponent.enabled = active;
        }

        // �Ǵ� ������ ���̶���Ʈ ������Ʈ Ȱ��ȭ/��Ȱ��ȭ
        Transform highlightObj = trainingDummy.transform.Find("Highlight");
        if (highlightObj != null)
        {
            highlightObj.gameObject.SetActive(active);
        }
    }

    // �÷��̾ ���̿� �����ߴ��� üũ
    private void Update()
    {
        if (tutorialComplete || !weaponEquipped) return;

        // �÷��̾ ���̿� ����� ������ �Դ��� Ȯ��
        if (trainingDummy != null && Vector3.Distance(GameInitializer.Instance.GetPlayerClass().playerTransform.position, trainingDummy.transform.position) < 3f)
        {
            // ���� Ʃ�丮�� ����
            tutorialComplete = true;
            StartCombatTutorial();
        }
    }

    // ���� Ʃ�丮�� ����
    private void StartCombatTutorial()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.StartDialog(nextDialogID);
        }
    }

    // Ʃ�丮�� ���� ���� üũ
    private void CheckTutorialProgress()
    {
        if (weaponEquipped && !tutorialComplete)
        {
            // ���� ���� �� ���� ���̶���Ʈ
            HighlightDummy();
        }
    }
}

