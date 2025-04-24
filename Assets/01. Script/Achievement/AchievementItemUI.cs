using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ���� ������ UI ��ũ��Ʈ
public class AchievementItemUI : MonoBehaviour
{
    [Header("UI ���")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image progressSlider;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Button rewardButton;
    [SerializeField] private GameObject completedIcon;
    [SerializeField] private GameObject hiddenPanel;
    [SerializeField] private TextMeshProUGUI percentText;

    private Achievement achievement;

    private void Start()
    {
        // ���� ��ư �̺�Ʈ ���
        if (rewardButton != null)
        {
            rewardButton.onClick.AddListener(ClaimReward);
        }
    }

    // ���� ���� ����
    public void SetAchievement(Achievement achievement)
    {
        this.achievement = achievement;

        // UI ������Ʈ
        UpdateUI();
    }

    // ������ �������� ����
    public void SetAsHidden()
    {
        // ������ ���� �г� Ȱ��ȭ
        if (hiddenPanel != null)
            hiddenPanel.SetActive(true);

        // �Ϲ� UI ��� ��Ȱ��ȭ
        if (nameText != null)
            nameText.text = "???";

        if (descriptionText != null)
            descriptionText.text = "???";

        if (progressText != null)
            progressText.text = "???";

        if (progressSlider != null)
            progressSlider.fillAmount = 0;

        if (rewardText != null)
            rewardText.text = "����: ???";

        if (rewardButton != null)
            rewardButton.gameObject.SetActive(false);

        if (completedIcon != null)
            completedIcon.SetActive(false);
    }

    // UI ������Ʈ
    private void UpdateUI()
    {
        if (achievement == null) return;

        // �̸� ����
        if (nameText != null)
            nameText.text = achievement.name;

        // ���� ����
        if (descriptionText != null)
            descriptionText.text = achievement.description;

        // ���൵ �ؽ�Ʈ ����
        if (progressText != null)
            progressText.text = $"���൵: {achievement.progressCurrent}/{achievement.progressRequired}";

        // ���൵ �����̴� ����
        if (progressSlider != null)
            progressSlider.fillAmount = (float)achievement.progressCurrent / achievement.progressRequired;

        if (percentText != null)
        {
            float percent = (float)achievement.progressCurrent / achievement.progressRequired * 100f;
            percent = Mathf.Clamp(percent, 0f, 100f); // 0~100 ���̷� ����
            percentText.text = $"{percent:0.0}%";
        }

        // ���� �ؽ�Ʈ ����
        if (rewardText != null)
        {
            rewardText.text = $"����: {achievement.GetRewardDescription()}";
        }

      

        // ���� ��ư ���� ����
        if (rewardButton != null)
        {
            rewardButton.gameObject.SetActive(achievement.isCompleted && !achievement.isRewardClaimed);
        }

        // �Ϸ� ������ ����
        if (completedIcon != null)
        {
            completedIcon.SetActive(achievement.isCompleted);
        }

        // ������ �г� ��Ȱ��ȭ (�Ϲ� ���� ǥ�� ��)
        if (hiddenPanel != null)
            hiddenPanel.SetActive(false);

        // �Ϸ�� ������ ��Ÿ�� ����
        if (achievement.isCompleted)
        {
            // �ؽ�Ʈ ���� ���� (�Ϸ�� ����)
            if (nameText != null)
                nameText.color = new Color(0.2f, 0.8f, 0.2f); // ��� �迭

            // �̹� ������ ���������� ǥ�� ����
            if (rewardText != null && achievement.isCompleted && achievement.isRewardClaimed)
            {
                rewardText.text = $"����: {achievement.GetRewardDescription()} (���� �Ϸ�)";
                rewardText.color = new Color(0.5f, 0.5f, 0.5f); // ȸ������ ����
            }
        }
        else
        {
            // �̿Ϸ� ���� �ؽ�Ʈ ���� (�⺻)
            if (nameText != null)
                nameText.color = Color.white;

            if (rewardText != null)
                rewardText.color = new Color(1f, 0.8f, 0.2f); // �ݻ� �迭
        }
    }

    // ���� ����
    private void ClaimReward()
    {
        if (achievement != null && AchievementManager.Instance != null)
        {
            AchievementManager.Instance.ClaimReward(achievement.id);

            // UI ������Ʈ
            UpdateUI();
        }
    }
}