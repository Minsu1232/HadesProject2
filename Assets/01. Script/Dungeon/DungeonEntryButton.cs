// ������ DungeonEntryButton
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DungeonEntryButton : MonoBehaviour
{
    [SerializeField] private string chapterId; // ��: "YasuoChapter"
    [SerializeField] private string dungeonSceneName = "DungeonScene";
    [SerializeField] private string startingStageID = "1_1"; // ��: 1_1, 2_1
    [SerializeField] private GameObject lockIcon; // ��� ������

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

        // é�� �ر� �̺�Ʈ ���� (FragmentChapterUnlocker�� �ִٸ�)
        if (FragmentChapterUnlocker.Instance != null)
        {
            FragmentChapterUnlocker.Instance.OnChapterUnlocked += OnChapterUnlocked;
        }
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
        }
    }

    // ��ư ���� ������Ʈ
    private void UpdateButtonState()
    {
        bool isUnlocked = true;

        // é�� �ر� Ȯ�� (SaveManager�� �ְ� chapterId�� �����Ǿ� �ִ� ���)
        if (!string.IsNullOrEmpty(chapterId) && SaveManager.Instance != null)
        {
            ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();
            if (chapterData != null)
            {
                isUnlocked = chapterData.IsChapterUnlocked(chapterId);
                Debug.Log(isUnlocked + " !@#!@#!@#");
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

    private void OnButtonClicked()
    {
        PlayerPrefs.SetString("CurrentStageID", startingStageID);
        PlayerPrefs.Save();

        // �ε� ȭ�� ���
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading(dungeonSceneName);
        }
        else
        {
            // �ε� ȭ���� ���� ��� ���� �� �ε�
            SceneManager.LoadScene(dungeonSceneName);
        }
    }
}