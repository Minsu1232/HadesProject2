// 수정된 DungeonEntryButton
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DungeonEntryButton : MonoBehaviour
{
    [SerializeField] private string chapterId; // 예: "YasuoChapter"
    [SerializeField] private string dungeonSceneName = "DungeonScene";
    [SerializeField] private string startingStageID = "1_1"; // 예: 1_1, 2_1
    [SerializeField] private GameObject lockIcon; // 잠금 아이콘

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

        // 챕터 해금 이벤트 구독 (FragmentChapterUnlocker가 있다면)
        if (FragmentChapterUnlocker.Instance != null)
        {
            FragmentChapterUnlocker.Instance.OnChapterUnlocked += OnChapterUnlocked;
        }
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
        }
    }

    // 버튼 상태 업데이트
    private void UpdateButtonState()
    {
        bool isUnlocked = true;

        // 챕터 해금 확인 (SaveManager가 있고 chapterId가 설정되어 있는 경우)
        if (!string.IsNullOrEmpty(chapterId) && SaveManager.Instance != null)
        {
            ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();
            if (chapterData != null)
            {
                isUnlocked = chapterData.IsChapterUnlocked(chapterId);
                Debug.Log(isUnlocked + " !@#!@#!@#");
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

    private void OnButtonClicked()
    {
        PlayerPrefs.SetString("CurrentStageID", startingStageID);
        PlayerPrefs.Save();

        // 로딩 화면 사용
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading(dungeonSceneName);
        }
        else
        {
            // 로딩 화면이 없는 경우 직접 씬 로드
            SceneManager.LoadScene(dungeonSceneName);
        }
    }
}