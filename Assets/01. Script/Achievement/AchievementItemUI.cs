using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 업적 아이템 UI 스크립트
public class AchievementItemUI : MonoBehaviour
{
    [Header("UI 요소")]
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
        // 보상 버튼 이벤트 등록
        if (rewardButton != null)
        {
            rewardButton.onClick.AddListener(ClaimReward);
        }
    }

    // 업적 정보 설정
    public void SetAchievement(Achievement achievement)
    {
        this.achievement = achievement;

        // UI 업데이트
        UpdateUI();
    }

    // 숨겨진 업적으로 설정
    public void SetAsHidden()
    {
        // 숨겨진 업적 패널 활성화
        if (hiddenPanel != null)
            hiddenPanel.SetActive(true);

        // 일반 UI 요소 비활성화
        if (nameText != null)
            nameText.text = "???";

        if (descriptionText != null)
            descriptionText.text = "???";

        if (progressText != null)
            progressText.text = "???";

        if (progressSlider != null)
            progressSlider.fillAmount = 0;

        if (rewardText != null)
            rewardText.text = "보상: ???";

        if (rewardButton != null)
            rewardButton.gameObject.SetActive(false);

        if (completedIcon != null)
            completedIcon.SetActive(false);
    }

    // UI 업데이트
    private void UpdateUI()
    {
        if (achievement == null) return;

        // 이름 설정
        if (nameText != null)
            nameText.text = achievement.name;

        // 설명 설정
        if (descriptionText != null)
            descriptionText.text = achievement.description;

        // 진행도 텍스트 설정
        if (progressText != null)
            progressText.text = $"진행도: {achievement.progressCurrent}/{achievement.progressRequired}";

        // 진행도 슬라이더 설정
        if (progressSlider != null)
            progressSlider.fillAmount = (float)achievement.progressCurrent / achievement.progressRequired;

        if (percentText != null)
        {
            float percent = (float)achievement.progressCurrent / achievement.progressRequired * 100f;
            percent = Mathf.Clamp(percent, 0f, 100f); // 0~100 사이로 제한
            percentText.text = $"{percent:0.0}%";
        }

        // 보상 텍스트 설정
        if (rewardText != null)
        {
            rewardText.text = $"보상: {achievement.GetRewardDescription()}";
        }

      

        // 보상 버튼 상태 설정
        if (rewardButton != null)
        {
            rewardButton.gameObject.SetActive(achievement.isCompleted && !achievement.isRewardClaimed);
        }

        // 완료 아이콘 설정
        if (completedIcon != null)
        {
            completedIcon.SetActive(achievement.isCompleted);
        }

        // 숨겨진 패널 비활성화 (일반 업적 표시 시)
        if (hiddenPanel != null)
            hiddenPanel.SetActive(false);

        // 완료된 업적에 스타일 적용
        if (achievement.isCompleted)
        {
            // 텍스트 색상 변경 (완료됨 강조)
            if (nameText != null)
                nameText.color = new Color(0.2f, 0.8f, 0.2f); // 녹색 계열

            // 이미 보상을 수령했으면 표시 변경
            if (rewardText != null && achievement.isCompleted && achievement.isRewardClaimed)
            {
                rewardText.text = $"보상: {achievement.GetRewardDescription()} (수령 완료)";
                rewardText.color = new Color(0.5f, 0.5f, 0.5f); // 회색으로 변경
            }
        }
        else
        {
            // 미완료 업적 텍스트 색상 (기본)
            if (nameText != null)
                nameText.color = Color.white;

            if (rewardText != null)
                rewardText.color = new Color(1f, 0.8f, 0.2f); // 금색 계열
        }
    }

    // 보상 수령
    private void ClaimReward()
    {
        if (achievement != null && AchievementManager.Instance != null)
        {
            AchievementManager.Instance.ClaimReward(achievement.id);

            // UI 업데이트
            UpdateUI();
        }
    }
}