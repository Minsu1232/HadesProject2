using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class DungeonEntryButton : MonoBehaviour
{
    [Header("é�� ����")]
    [SerializeField] private string chapterId; // ��: "YasuoChapter"
    [SerializeField] private string dungeonSceneName = "Chapter1Dungeon";
    [SerializeField] private string startingStageID = "1_1"; // ��: 1_1, 2_1
    [SerializeField] private int chapterNumber = 1; // é�� ��ȣ (ǥ�ÿ�)

    [Header("UI ���")]
    [SerializeField] private GameObject lockIcon; // ��� ������   
    [SerializeField] private TextMeshProUGUI chapterNumberText; // é�� ��ȣ �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI attemptsText; // �õ� Ƚ�� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI bestRecordText; // �ְ� ��� �ؽ�Ʈ

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    private void Start()
    {
        // �ʱ� ���� ����
        UpdateButtonState();
        UpdateChapterInfo(); // ���� �� ���� ������Ʈ

        // é�� �ر� �̺�Ʈ ���� (FragmentChapterUnlocker�� �ִٸ�)
        if (FragmentChapterUnlocker.Instance != null)
        {
            FragmentChapterUnlocker.Instance.OnChapterUnlocked += OnChapterUnlocked;
        }
    }

    private void OnEnable()
    {
        // UI�� Ȱ��ȭ�� ������ ���� ����
        UpdateChapterInfo();
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (FragmentChapterUnlocker.Instance != null)
        {
            FragmentChapterUnlocker.Instance.OnChapterUnlocked -= OnChapterUnlocked;
        }
    }

    // é�� �ر� �� ȣ��
    private void OnChapterUnlocked(string unlockedChapterId, string chapterName)
    {
        if (unlockedChapterId == chapterId)
        {
            UpdateButtonState();
            UpdateChapterInfo();
        }
    }

    // ��ư ���� ������Ʈ
    private void UpdateButtonState()
    {
        bool isUnlocked = true;
        // é�� �ر� Ȯ��
        if (!string.IsNullOrEmpty(chapterId) && SaveManager.Instance != null)
        {
            ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();
            if (chapterData != null)
            {
                isUnlocked = chapterData.IsChapterUnlocked(chapterId);
            }
        }

        // ��ư Ȱ��ȭ/��Ȱ��ȭ
        button.interactable = isUnlocked;

        // ��� ������ ǥ��/����
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }
    }

    // é�� ���� ������Ʈ (�õ� Ƚ��, �ְ� ���)
    private void UpdateChapterInfo()
    {
        // é�� ��ȣ ǥ��
        if (chapterNumberText != null)
        {
            chapterNumberText.text = $"é�� {chapterNumber}";
        }

        if (SaveManager.Instance == null || string.IsNullOrEmpty(chapterId))
            return;

        ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();
        if (chapterData == null)
            return;

        // �õ� Ƚ�� ǥ��
        int attempts = chapterData.GetChapterAttempts(chapterId);
        if (attemptsText != null)
        {
            attemptsText.text = $"�õ� ȸ��: {attempts}";
        }

        // �ְ� ��� ǥ��
        string bestRecord = chapterData.GetChapterBestRecord(chapterId);
        if (bestRecordText != null)
        {
            if (string.IsNullOrEmpty(bestRecord))
            {
                bestRecordText.text = $"�ְ� ���: -";
            }
            else
            {
                bestRecordText.text = $"�ְ� ���: {bestRecord}";
            }
        }
    }

    private void OnButtonClicked()
    {
        // ���� �������� ID�� é�� ID ����
        PlayerPrefs.SetString("CurrentStageID", startingStageID);
        PlayerPrefs.SetString("CurrentChapterID", chapterId);
        PlayerPrefs.Save();

        // �ε� ȭ�� ���
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