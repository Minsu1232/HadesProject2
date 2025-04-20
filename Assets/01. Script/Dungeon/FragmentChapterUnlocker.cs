using System;
using System.Collections.Generic;
using UnityEngine;

public class FragmentChapterUnlocker : MonoBehaviour
{
    public static FragmentChapterUnlocker Instance { get; private set; }

    // ���� ID�� �ر��� é�� ID ����
    [Serializable]
    public class FragmentChapterMapping
    {
        public int fragmentId;
        public string chapterId;
        public string chapterName; // UI ǥ�ÿ�
    }

    [SerializeField]
    private List<FragmentChapterMapping> fragmentChapterMappings = new List<FragmentChapterMapping>()
    {
        new FragmentChapterMapping { fragmentId = 1001, chapterId = "YongzokChapter", chapterName = "������ ����" },
        new FragmentChapterMapping { fragmentId = 1002, chapterId = "DeathChapter", chapterName = "������ ��" },
        new FragmentChapterMapping { fragmentId = 1003, chapterId = "HeartChapter", chapterName = "�����" }
    };

    // �׼��� ����� é�� �ر� �̺�Ʈ
    public event Action<string, string> OnChapterUnlocked; // chapterId, chapterName

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // �κ��丮 �ý��� �̺�Ʈ ����
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnItemAdded += CheckFragmentForChapterUnlock;
        }
        else
        {
            Debug.LogError("InventorySystem�� ã�� �� �����ϴ�!");
        }
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnItemAdded -= CheckFragmentForChapterUnlock;
        }
    }

    // ������ �߰��� �� �������� Ȯ���ϰ� é�� �ر�
    public void CheckFragmentForChapterUnlock(int itemId, int quantity)
    {
        // �� �������� �ر� ������ �������� Ȯ��
        FragmentChapterMapping mapping = fragmentChapterMappings.Find(m => m.fragmentId == itemId);

        if (mapping != null)
        {
            Debug.Log($"���� ���� {itemId} ȹ�� ����! é�� �ر� �õ�: {mapping.chapterId}");

            // é�� ���α׷��� ������ ��������
            ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();

            if (chapterData != null)
            {
                // �̹� �رݵǾ����� Ȯ��
                if (!chapterData.IsChapterUnlocked(mapping.chapterId))
                {
                    // é�� �ر�
                    UnlockChapter(mapping.chapterId, mapping.chapterName);
                }
                else
                {
                    Debug.Log($"é�� {mapping.chapterId}�� �̹� �رݵǾ� �ֽ��ϴ�.");
                }
            }
            else
            {
                Debug.LogError("ChapterProgressData�� ������ �� �����ϴ�!");
            }
        }
    }

    // é�� �ر� ó��
    public void UnlockChapter(string chapterId, string chapterName)
    {
        ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();

        if (chapterData != null)
        {
            // é�� �ر� ���� ������Ʈ
            ChapterProgressData.ChapterData chapter = chapterData.GetChapterData(chapterId);

            if (chapter != null)
            {
                chapter.isUnlocked = true;
                SaveManager.Instance.SaveChapterData();

                Debug.Log($"é�� �ر� ����: {chapterId} ({chapterName})");

                // �̺�Ʈ �߻�
                OnChapterUnlocked?.Invoke(chapterId, chapterName);

                // UI �˸� ǥ�� (UIManager�� �ִٸ�)
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowNotification($"���ο� é�Ͱ� �رݵǾ����ϴ�: {chapterName}",Color.magenta);
                }
            }
            else
            {
                Debug.LogWarning($"é�� ID {chapterId}�� �ش��ϴ� é�� �����͸� ã�� �� �����ϴ�.");
            }
        }
    }

    // fragmentId�� �ش��ϴ� chapterId ��ȯ
    public string GetChapterIdForFragment(int fragmentId)
    {
        FragmentChapterMapping mapping = fragmentChapterMappings.Find(m => m.fragmentId == fragmentId);
        return mapping?.chapterId;
    }

    // chapterId�� �ش��ϴ� fragmentId ��ȯ
    public int GetFragmentIdForChapter(string chapterId)
    {
        FragmentChapterMapping mapping = fragmentChapterMappings.Find(m => m.chapterId == chapterId);
        return mapping != null ? mapping.fragmentId : -1;
    }
}