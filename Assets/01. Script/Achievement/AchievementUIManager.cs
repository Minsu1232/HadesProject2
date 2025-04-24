using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementUIManager : MonoBehaviour
{
    [Header("�г� ����")]
    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private GameObject openButton;
    [SerializeField] private Button closeButton;

    [Header("ī�װ� ��")]
    [SerializeField] private Button[] categoryButtons;
    [SerializeField] private Color selectedTabColor = new Color(1f, 1f, 0.5f);
    [SerializeField] private Color unselectedTabColor = new Color(1f, 1f, 0.8f);

    [Header("���� ���")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject achievementItemPrefab;
    [SerializeField] private GridLayoutGroup gridLayout; // �׸��� ���̾ƿ� (�¿� ��ġ��)

    [Header("���� ��� ����")]
    [SerializeField] private TextMeshProUGUI completionText; // ���� �Ϸ��� �ؽ�Ʈ

    private AchievementCategory currentCategory = AchievementCategory.Combat;
    private AchievementManager achievementManager;

    private void Start()
    {
        achievementManager = AchievementManager.Instance;

        if (achievementManager == null)
        {
            Debug.LogError("AchievementManager�� ã�� �� �����ϴ�!");
            return;
        }

        // �ʱ�ȭ �� �г� ��Ȱ��ȭ
        if (achievementPanel != null)
            achievementPanel.SetActive(false);

        // ī�װ� ��ư �̺�Ʈ ���
        if (categoryButtons != null)
        {
            for (int i = 0; i < categoryButtons.Length && i < System.Enum.GetValues(typeof(AchievementCategory)).Length; i++)
            {
                AchievementCategory category = (AchievementCategory)i;
                int index = i; // Ŭ���� ���� ������ ���� ����

                if (categoryButtons[i] != null)
                {
                    categoryButtons[i].onClick.AddListener(() => SelectCategory(category));
                }
            }
        }

        // �ݱ� ��ư �̺�Ʈ ���
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseAchievementPanel);

        // ���� ���� �̺�Ʈ ������ ���
        if (achievementManager != null)
        {
            achievementManager.OnAchievementCompleted += OnAchievementCompleted;
            //achievementManager.OnAchievementProgressChanged += OnAchievementProgressChanged;
            achievementManager.OnAchievementDataRefreshed += UpdateAchievementList;
        }

        // �׸��� ���̾ƿ� ���� (�¿� ��ġ)
        if (gridLayout != null)
        {
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2; // 2�� ����
        }
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ������ ����
        if (achievementManager != null)
        {
            achievementManager.OnAchievementCompleted -= OnAchievementCompleted;
            //achievementManager.OnAchievementProgressChanged -= OnAchievementProgressChanged;
            achievementManager.OnAchievementDataRefreshed -= UpdateAchievementList;
        }
    }

    // ���� �Ϸ� �̺�Ʈ ó��
    private void OnAchievementCompleted(Achievement achievement)
    {
        // ���� ���� �ִ� ī�װ��� �����ϸ� ���� ��� ����
        if (achievement.category == currentCategory)
        {
            UpdateAchievementList();
        }

        // ���� �Ϸ��� ����
        UpdateCompletionRate();
    }

    // ���� ���൵ ���� �̺�Ʈ ó��
    private void OnAchievementProgressChanged(Achievement achievement, int previousProgress, int currentProgress)
    {
        // ���� ���� �ִ� ī�װ��� �����ϸ� ���� ��� ����
        if (achievement.category == currentCategory)
        {
            UpdateAchievementList();
        }
    }

    // ���� �г� ����
    public void OpenAchievementPanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(true);

            // UI ���ÿ� ��� (UIManager�� �ִ� ���)
            UIManager.Instance?.RegisterActiveUI(achievementPanel);

            // �ʱ� ī�װ� ���� �� ���� ��� ������Ʈ
            SelectCategory(currentCategory);

            // ���� �Ϸ��� ����
            UpdateCompletionRate();
        }
    }

    // ���� �г� �ݱ�
    public void CloseAchievementPanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);

            // UI ���ÿ��� ���� (UIManager�� �ִ� ���)
            UIManager.Instance?.UnregisterActiveUI(achievementPanel);
        }
    }

    // ī�װ� ����
    private void SelectCategory(AchievementCategory category)
    {
        currentCategory = category;

        // ī�װ� ��ư ���� ������Ʈ
        UpdateCategoryButtons();

        // ���� ��� ������Ʈ
        UpdateAchievementList();
    }

    // ī�װ� ��ư ���� ������Ʈ
    private void UpdateCategoryButtons()
    {
        if (categoryButtons == null) return;

        for (int i = 0; i < categoryButtons.Length && i < System.Enum.GetValues(typeof(AchievementCategory)).Length; i++)
        {
            bool isSelected = (AchievementCategory)i == currentCategory;

            if (categoryButtons[i] != null)
            {
                // ��ư ���� ����
                Image buttonImage = categoryButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = isSelected ? selectedTabColor : unselectedTabColor;
                }

                // �ؽ�Ʈ ���� (�ִ� ���)
                TextMeshProUGUI buttonText = categoryButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
                }
            }
        }
    }

    // ���� ��� ������Ʈ
    private void UpdateAchievementList()
    {
        if (contentParent == null || achievementItemPrefab == null || achievementManager == null) return;

        // ���� ��� ����
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // ���õ� ī�װ��� ���� ��������
        List<Achievement> categoryAchievements = achievementManager.GetAchievementsByCategory(currentCategory);

        // ���� ������ ����
        foreach (Achievement achievement in categoryAchievements)
        {
            // ������ �����̰� �޼����� �ʾ����� ������ ���·� ǥ��
            GameObject itemObj = Instantiate(achievementItemPrefab, contentParent);
            AchievementItemUI itemUI = itemObj.GetComponent<AchievementItemUI>();

            if (itemUI != null)
            {
                if (achievement.isHidden && !achievement.isCompleted)
                {
                    itemUI.SetAsHidden();
                }
                else
                {
                    itemUI.SetAchievement(achievement);
                }
            }
        }
    }

    // ���� �Ϸ��� ����
    private void UpdateCompletionRate()
    {
        if (completionText != null && achievementManager != null)
        {
            int completed = achievementManager.GetCompletedAchievementCount();
            int total = achievementManager.GetTotalAchievementCount();
            float percentage = achievementManager.GetCompletionPercentage();

            completionText.text = $"���� �޼�: {completed}/{total} ({percentage:F1}%)";
        }
    }

    // ī�װ� �̸� �������� (UI ǥ�ÿ�)
    private string GetCategoryName(AchievementCategory category)
    {
        switch (category)
        {
            case AchievementCategory.Combat:
                return "����";
            case AchievementCategory.Survival:
                return "����";
            case AchievementCategory.Progress:
                return "���൵";
            case AchievementCategory.Upgrade:
                return "��ȭ";
            case AchievementCategory.Discovery:
                return "�߰�";
            default:
                return category.ToString();
        }
    }
}