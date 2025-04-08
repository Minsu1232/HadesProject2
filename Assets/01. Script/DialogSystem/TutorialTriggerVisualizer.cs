using UnityEngine;

public class TutorialTriggerVisualizer : MonoBehaviour
{
    [Header("�ð��� ���")]
    [SerializeField] private GameObject visualIndicatorPrefab; // �ٴڿ� ǥ�õ� ����Ʈ
    [SerializeField] private float indicatorHeight = 0.1f; // �ٴ� �� ����
    [SerializeField] private Color indicatorColor = new Color(0.2f, 0.8f, 0.2f, 0.4f); // ������ ���

    [Header("Ȱ��ȭ ����")]
    [SerializeField] private string requiredFlag = ""; // �� �÷��װ� ���� ���� ǥ�� (��������� �׻� ǥ��)
    [SerializeField] private string hideAfterFlag = ""; // �� �÷��װ� ������ ����

    private GameObject indicatorInstance;
    private bool isActive = false;

    private void Start()
    {
        CheckConditionAndShow();

        // ���̾�α� �̺�Ʈ ���� (�÷��� ���� ������)
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

    // ���̾�α� �̺�Ʈ ó�� (�÷��� ���� ����)
    private void OnDialogEvent(string eventName)
    {
        // �÷��� ���� �̺�Ʈ���� Ȯ��
        if (eventName.StartsWith("SetFlag:"))
        {
            CheckConditionAndShow();
        }
    }

    // Ʈ���ſ� ������ �� ������ �޼���
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isActive)
        {
            // �ݶ��̴��� ����ϸ� �ð��� ǥ�� ����
            HideIndicator();

            // ���� hideAfterFlag�� ������ �÷��� ����
            if (!string.IsNullOrEmpty(hideAfterFlag))
            {
                GameProgressManager.Instance?.SetFlag(hideAfterFlag, true);
            }
        }
    }

    // ������ Ȯ���ϰ� �ð��� ��� ǥ�� ���� ����
    private void CheckConditionAndShow()
    {
        bool shouldShow = true;

        // requiredFlag Ȯ�� (������ �÷��װ� Ȱ��ȭ�Ǿ�� ǥ��)
        if (!string.IsNullOrEmpty(requiredFlag))
        {
            shouldShow = GameProgressManager.Instance?.GetFlag(requiredFlag) ?? false;
        }

        // hideAfterFlag Ȯ�� (������ �÷��װ� Ȱ��ȭ�Ǿ��� �� ����)
        if (!string.IsNullOrEmpty(hideAfterFlag))
        {
            shouldShow = shouldShow && !(GameProgressManager.Instance?.GetFlag(hideAfterFlag) ?? false);
        }

        // ���� ���� �ÿ��� ó��
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

    // �ð��� ǥ�� ����
    private void ShowIndicator()
    {
        if (visualIndicatorPrefab != null && indicatorInstance == null)
        {
            // �ݶ��̴� ũ�� ��������
            Collider col = GetComponent<Collider>();
            Vector3 size = col != null ? col.bounds.size : new Vector3(2, 0.1f, 2);
            Vector3 position = transform.position + new Vector3(0, indicatorHeight, 0);

            // �ٴڿ� ǥ�õ� ����Ʈ ����
            indicatorInstance = Instantiate(visualIndicatorPrefab, position, Quaternion.identity);
            indicatorInstance.transform.parent = transform;

            // ũ�� ���� (�ݶ��̴� ũ�⿡ �°�)
            indicatorInstance.transform.localScale = new Vector3(size.x, 1, size.z);

            // ���� ����
            Renderer renderer = indicatorInstance.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = indicatorColor;
            }
        }
    }

    // �ð��� ǥ�� ����
    private void HideIndicator()
    {
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
            indicatorInstance = null;
        }
    }
}