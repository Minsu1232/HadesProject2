// ChapterSelectUI.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChapterSelectUI : MonoBehaviour
{
    [Serializable]
    public class ChapterButtonInfo
    {
        public string chapterId;
        public Button button;
        public GameObject lockIcon;
        public TextMeshProUGUI chapterNameText;
        public TextMeshProUGUI requirementText;
    }

    [SerializeField] private List<ChapterButtonInfo> chapterButtons;
    [SerializeField] private GameObject unlockEffectPrefab; // �ر� ����Ʈ

    private void Start()
    {
        // �ʱ� UI ���� ����
        UpdateChapterButtons();

        // é�� �ر� �̺�Ʈ ����
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

    // é�� �ر� �� ȣ��� �޼���
    private void OnChapterUnlocked(string chapterId, string chapterName)
    {
        // �ش� é�� ��ư ã��
        ChapterButtonInfo buttonInfo = chapterButtons.Find(b => b.chapterId == chapterId);

        if (buttonInfo != null)
        {
            // ��ư Ȱ��ȭ
            buttonInfo.button.interactable = true;

            // ��� ������ �����
            if (buttonInfo.lockIcon != null)
            {
                buttonInfo.lockIcon.SetActive(false);
            }

            // �䱸���� �ؽ�Ʈ �����
            if (buttonInfo.requirementText != null)
            {
                buttonInfo.requirementText.gameObject.SetActive(false);
            }

            // �ر� ����Ʈ ��� (�ִٸ�)
            if (unlockEffectPrefab != null)
            {
                Instantiate(unlockEffectPrefab, buttonInfo.button.transform.position, Quaternion.identity, transform);
            }
        }

        // ��ü UI ������Ʈ
        UpdateChapterButtons();
    }

    // é�� ��ư ���� ������Ʈ
    private void UpdateChapterButtons()
    {
        ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();

        if (chapterData == null)
        {
            Debug.LogError("ChapterProgressData�� ������ �� �����ϴ�!");
            return;
        }

        foreach (var buttonInfo in chapterButtons)
        {
            bool isUnlocked = chapterData.IsChapterUnlocked(buttonInfo.chapterId);

            // ��ư Ȱ��ȭ/��Ȱ��ȭ
            buttonInfo.button.interactable = isUnlocked;

            // ��� ������ ǥ��/����
            if (buttonInfo.lockIcon != null)
            {
                buttonInfo.lockIcon.SetActive(!isUnlocked);
            }

            // �رݵ��� ���� ��� �ʿ��� ���� ǥ��
            if (!isUnlocked && buttonInfo.requirementText != null)
            {
                int fragmentId = FragmentChapterUnlocker.Instance.GetFragmentIdForChapter(buttonInfo.chapterId);

                if (fragmentId > 0)
                {
                    Item fragmentItem = ItemDataManager.Instance.GetItem(fragmentId);
                    if (fragmentItem != null)
                    {
                        buttonInfo.requirementText.text = $"�ʿ�: {fragmentItem.itemName}";
                        buttonInfo.requirementText.gameObject.SetActive(true);
                    }
                    else
                    {
                        buttonInfo.requirementText.text = "??? ���� �ʿ�";
                        buttonInfo.requirementText.gameObject.SetActive(true);
                    }
                }
            }
            else if (buttonInfo.requirementText != null)
            {
                buttonInfo.requirementText.gameObject.SetActive(false);
            }
        }
    }

    // é�� ��ư Ŭ�� ó��
    public void OnChapterButtonClicked(string chapterId)
    {
        Debug.Log($"é�� {chapterId} ���õ�");

        // é�ͺ� ù �������� ID ��������
        string firstStageId = GetFirstStageId(chapterId);

        if (!string.IsNullOrEmpty(firstStageId))
        {
            // ���õ� �������� ID ����
            PlayerPrefs.SetString("CurrentStageID", firstStageId);
            PlayerPrefs.Save();

            // ���� �� �ε�
            string dungeonSceneName = GetDungeonSceneForChapter(chapterId);
            LoadingScreen.Instance.ShowLoading(dungeonSceneName);
        }
        else
        {
            Debug.LogError($"é�� {chapterId}�� ù �������� ID�� ������ �� �����ϴ�!");
        }
    }

    // é�ͺ� ù �������� ID ��������
    private string GetFirstStageId(string chapterId)
    {
        // é�ͺ� ù �������� ����
        switch (chapterId)
        {
            case "YasuoChapter": return "1_1";
            case "YongzokChapter": return "2_1";
            case "DeathChapter": return "3_1";
            case "HeartChapter": return "4_1";
            default: return "";
        }
    }

    // é�ͺ� ���� �� �̸� ��������
    private string GetDungeonSceneForChapter(string chapterId)
    {
        // é�ͺ� ���� �� ����
        switch (chapterId)
        {
            case "YasuoChapter": return "Chapter1Dungeon";
            case "YongzokChapter": return "Chapter2Dungeon";
            case "DeathChapter": return "Chapter3Dungeon";
            case "HeartChapter": return "Chapter4Dungeon";
            default: return "DungeonScene"; // �⺻��
        }
    }
}