using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementUIManager : MonoBehaviour
{
    [Header("패널 설정")]
    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private GameObject openButton;
    [SerializeField] private Button closeButton;

    [Header("카테고리 탭")]
    [SerializeField] private Button[] categoryButtons;
    [SerializeField] private Color selectedTabColor = new Color(1f, 1f, 0.5f);
    [SerializeField] private Color unselectedTabColor = new Color(1f, 1f, 0.8f);

    [Header("업적 목록")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject achievementItemPrefab;
    [SerializeField] private GridLayoutGroup gridLayout; // 그리드 레이아웃 (좌우 배치용)

    [Header("업적 요약 정보")]
    [SerializeField] private TextMeshProUGUI completionText; // 업적 완료율 텍스트

    private AchievementCategory currentCategory = AchievementCategory.Combat;
    private AchievementManager achievementManager;

    private void Start()
    {
        achievementManager = AchievementManager.Instance;

        if (achievementManager == null)
        {
            Debug.LogError("AchievementManager를 찾을 수 없습니다!");
            return;
        }

        // 초기화 시 패널 비활성화
        if (achievementPanel != null)
            achievementPanel.SetActive(false);

        // 카테고리 버튼 이벤트 등록
        if (categoryButtons != null)
        {
            for (int i = 0; i < categoryButtons.Length && i < System.Enum.GetValues(typeof(AchievementCategory)).Length; i++)
            {
                AchievementCategory category = (AchievementCategory)i;
                int index = i; // 클로저 문제 방지용 로컬 변수

                if (categoryButtons[i] != null)
                {
                    categoryButtons[i].onClick.AddListener(() => SelectCategory(category));
                }
            }
        }

        // 닫기 버튼 이벤트 등록
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseAchievementPanel);

        // 업적 관련 이벤트 리스너 등록
        if (achievementManager != null)
        {
            achievementManager.OnAchievementCompleted += OnAchievementCompleted;
            //achievementManager.OnAchievementProgressChanged += OnAchievementProgressChanged;
            achievementManager.OnAchievementDataRefreshed += UpdateAchievementList;
        }

        // 그리드 레이아웃 설정 (좌우 배치)
        if (gridLayout != null)
        {
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2; // 2열 구조
        }
    }

    private void OnDestroy()
    {
        // 이벤트 리스너 해제
        if (achievementManager != null)
        {
            achievementManager.OnAchievementCompleted -= OnAchievementCompleted;
            //achievementManager.OnAchievementProgressChanged -= OnAchievementProgressChanged;
            achievementManager.OnAchievementDataRefreshed -= UpdateAchievementList;
        }
    }

    // 업적 완료 이벤트 처리
    private void OnAchievementCompleted(Achievement achievement)
    {
        // 현재 보고 있는 카테고리와 동일하면 업적 목록 갱신
        if (achievement.category == currentCategory)
        {
            UpdateAchievementList();
        }

        // 업적 완료율 갱신
        UpdateCompletionRate();
    }

    // 업적 진행도 변경 이벤트 처리
    private void OnAchievementProgressChanged(Achievement achievement, int previousProgress, int currentProgress)
    {
        // 현재 보고 있는 카테고리와 동일하면 업적 목록 갱신
        if (achievement.category == currentCategory)
        {
            UpdateAchievementList();
        }
    }

    // 업적 패널 열기
    public void OpenAchievementPanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(true);

            // UI 스택에 등록 (UIManager가 있는 경우)
            UIManager.Instance?.RegisterActiveUI(achievementPanel);

            // 초기 카테고리 설정 및 업적 목록 업데이트
            SelectCategory(currentCategory);

            // 업적 완료율 갱신
            UpdateCompletionRate();
        }
    }

    // 업적 패널 닫기
    public void CloseAchievementPanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);

            // UI 스택에서 제거 (UIManager가 있는 경우)
            UIManager.Instance?.UnregisterActiveUI(achievementPanel);
        }
    }

    // 카테고리 선택
    private void SelectCategory(AchievementCategory category)
    {
        currentCategory = category;

        // 카테고리 버튼 상태 업데이트
        UpdateCategoryButtons();

        // 업적 목록 업데이트
        UpdateAchievementList();
    }

    // 카테고리 버튼 상태 업데이트
    private void UpdateCategoryButtons()
    {
        if (categoryButtons == null) return;

        for (int i = 0; i < categoryButtons.Length && i < System.Enum.GetValues(typeof(AchievementCategory)).Length; i++)
        {
            bool isSelected = (AchievementCategory)i == currentCategory;

            if (categoryButtons[i] != null)
            {
                // 버튼 색상 변경
                Image buttonImage = categoryButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = isSelected ? selectedTabColor : unselectedTabColor;
                }

                // 텍스트 강조 (있는 경우)
                TextMeshProUGUI buttonText = categoryButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
                }
            }
        }
    }

    // 업적 목록 업데이트
    private void UpdateAchievementList()
    {
        if (contentParent == null || achievementItemPrefab == null || achievementManager == null) return;

        // 기존 목록 제거
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 선택된 카테고리의 업적 가져오기
        List<Achievement> categoryAchievements = achievementManager.GetAchievementsByCategory(currentCategory);

        // 업적 아이템 생성
        foreach (Achievement achievement in categoryAchievements)
        {
            // 숨겨진 업적이고 달성하지 않았으면 숨겨진 형태로 표시
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

    // 업적 완료율 갱신
    private void UpdateCompletionRate()
    {
        if (completionText != null && achievementManager != null)
        {
            int completed = achievementManager.GetCompletedAchievementCount();
            int total = achievementManager.GetTotalAchievementCount();
            float percentage = achievementManager.GetCompletionPercentage();

            completionText.text = $"업적 달성: {completed}/{total} ({percentage:F1}%)";
        }
    }

    // 카테고리 이름 가져오기 (UI 표시용)
    private string GetCategoryName(AchievementCategory category)
    {
        switch (category)
        {
            case AchievementCategory.Combat:
                return "전투";
            case AchievementCategory.Survival:
                return "생존";
            case AchievementCategory.Progress:
                return "진행도";
            case AchievementCategory.Upgrade:
                return "강화";
            case AchievementCategory.Discovery:
                return "발견";
            default:
                return category.ToString();
        }
    }
}