using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class DungeonEntryButton : MonoBehaviour
{
    [Header("챕터 설정")]
    [SerializeField] private string chapterId; // 예: "YasuoChapter"
    [SerializeField] private string dungeonSceneName = "Chapter1Dungeon";
    [SerializeField] private string startingStageID = "1_1"; // 예: 1_1, 2_1
    [SerializeField] private int chapterNumber = 1; // 챕터 번호 (표시용)

    [Header("UI 요소")]
    [SerializeField] private GameObject lockIcon; // 잠금 아이콘   
    [SerializeField] private TextMeshProUGUI chapterNumberText; // 챕터 번호 텍스트
    [SerializeField] private TextMeshProUGUI attemptsText; // 시도 횟수 텍스트
    [SerializeField] private TextMeshProUGUI bestRecordText; // 최고 기록 텍스트

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    private void Start()
    {
        // 초기 상태 설정
        UpdateButtonState();
        UpdateChapterInfo(); // 시작 시 정보 업데이트

        // 챕터 해금 이벤트 구독 (FragmentChapterUnlocker가 있다면)
        if (FragmentChapterUnlocker.Instance != null)
        {
            FragmentChapterUnlocker.Instance.OnChapterUnlocked += OnChapterUnlocked;
        }
    }

    private void OnEnable()
    {
        // UI가 활성화될 때마다 정보 갱신
        UpdateChapterInfo();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (FragmentChapterUnlocker.Instance != null)
        {
            FragmentChapterUnlocker.Instance.OnChapterUnlocked -= OnChapterUnlocked;
        }
    }

    // 챕터 해금 시 호출
    private void OnChapterUnlocked(string unlockedChapterId, string chapterName)
    {
        if (unlockedChapterId == chapterId)
        {
            UpdateButtonState();
            UpdateChapterInfo();
        }
    }

    // 버튼 상태 업데이트
    private void UpdateButtonState()
    {
        bool isUnlocked = true;
        // 챕터 해금 확인
        if (!string.IsNullOrEmpty(chapterId) && SaveManager.Instance != null)
        {
            ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();
            if (chapterData != null)
            {
                isUnlocked = chapterData.IsChapterUnlocked(chapterId);
            }
        }

        // 버튼 활성화/비활성화
        button.interactable = isUnlocked;

        // 잠금 아이콘 표시/숨김
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }
    }

    // 챕터 정보 업데이트 (시도 횟수, 최고 기록)
    private void UpdateChapterInfo()
    {
        // 챕터 번호 표시
        if (chapterNumberText != null)
        {
            chapterNumberText.text = $"챕터 {chapterNumber}";
        }

        if (SaveManager.Instance == null || string.IsNullOrEmpty(chapterId))
            return;

        ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();
        if (chapterData == null)
            return;

        // 시도 횟수 표시
        int attempts = chapterData.GetChapterAttempts(chapterId);
        if (attemptsText != null)
        {
            attemptsText.text = $"시도 회수: {attempts}";
        }

        // 최고 기록 표시
        string bestRecord = chapterData.GetChapterBestRecord(chapterId);
        if (bestRecordText != null)
        {
            if (string.IsNullOrEmpty(bestRecord))
            {
                bestRecordText.text = $"최고 기록: -";
            }
            else
            {
                bestRecordText.text = $"최고 기록: {bestRecord}";
            }
        }
    }

    private void OnButtonClicked()
    {
        // 현재 스테이지 ID와 챕터 ID 저장
        PlayerPrefs.SetString("CurrentStageID", startingStageID);
        PlayerPrefs.SetString("CurrentChapterID", chapterId);
        PlayerPrefs.Save();

        // 로딩 화면 사용
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading(dungeonSceneName);
        }
        else
        {
            SceneManager.LoadScene(dungeonSceneName);
        }
    }
}