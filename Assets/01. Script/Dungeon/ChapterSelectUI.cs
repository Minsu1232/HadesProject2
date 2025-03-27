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
    [SerializeField] private GameObject unlockEffectPrefab; // 해금 이펙트

    private void Start()
    {
        // 초기 UI 상태 설정
        UpdateChapterButtons();

        // 챕터 해금 이벤트 구독
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

    // 챕터 해금 시 호출될 메서드
    private void OnChapterUnlocked(string chapterId, string chapterName)
    {
        // 해당 챕터 버튼 찾기
        ChapterButtonInfo buttonInfo = chapterButtons.Find(b => b.chapterId == chapterId);

        if (buttonInfo != null)
        {
            // 버튼 활성화
            buttonInfo.button.interactable = true;

            // 잠금 아이콘 숨기기
            if (buttonInfo.lockIcon != null)
            {
                buttonInfo.lockIcon.SetActive(false);
            }

            // 요구사항 텍스트 숨기기
            if (buttonInfo.requirementText != null)
            {
                buttonInfo.requirementText.gameObject.SetActive(false);
            }

            // 해금 이펙트 재생 (있다면)
            if (unlockEffectPrefab != null)
            {
                Instantiate(unlockEffectPrefab, buttonInfo.button.transform.position, Quaternion.identity, transform);
            }
        }

        // 전체 UI 업데이트
        UpdateChapterButtons();
    }

    // 챕터 버튼 상태 업데이트
    private void UpdateChapterButtons()
    {
        ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();

        if (chapterData == null)
        {
            Debug.LogError("ChapterProgressData를 가져올 수 없습니다!");
            return;
        }

        foreach (var buttonInfo in chapterButtons)
        {
            bool isUnlocked = chapterData.IsChapterUnlocked(buttonInfo.chapterId);

            // 버튼 활성화/비활성화
            buttonInfo.button.interactable = isUnlocked;

            // 잠금 아이콘 표시/숨김
            if (buttonInfo.lockIcon != null)
            {
                buttonInfo.lockIcon.SetActive(!isUnlocked);
            }

            // 해금되지 않은 경우 필요한 파편 표시
            if (!isUnlocked && buttonInfo.requirementText != null)
            {
                int fragmentId = FragmentChapterUnlocker.Instance.GetFragmentIdForChapter(buttonInfo.chapterId);

                if (fragmentId > 0)
                {
                    Item fragmentItem = ItemDataManager.Instance.GetItem(fragmentId);
                    if (fragmentItem != null)
                    {
                        buttonInfo.requirementText.text = $"필요: {fragmentItem.itemName}";
                        buttonInfo.requirementText.gameObject.SetActive(true);
                    }
                    else
                    {
                        buttonInfo.requirementText.text = "??? 파편 필요";
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

    // 챕터 버튼 클릭 처리
    public void OnChapterButtonClicked(string chapterId)
    {
        Debug.Log($"챕터 {chapterId} 선택됨");

        // 챕터별 첫 스테이지 ID 가져오기
        string firstStageId = GetFirstStageId(chapterId);

        if (!string.IsNullOrEmpty(firstStageId))
        {
            // 선택된 스테이지 ID 저장
            PlayerPrefs.SetString("CurrentStageID", firstStageId);
            PlayerPrefs.Save();

            // 던전 씬 로드
            string dungeonSceneName = GetDungeonSceneForChapter(chapterId);
            LoadingScreen.Instance.ShowLoading(dungeonSceneName);
        }
        else
        {
            Debug.LogError($"챕터 {chapterId}의 첫 스테이지 ID를 가져올 수 없습니다!");
        }
    }

    // 챕터별 첫 스테이지 ID 가져오기
    private string GetFirstStageId(string chapterId)
    {
        // 챕터별 첫 스테이지 매핑
        switch (chapterId)
        {
            case "YasuoChapter": return "1_1";
            case "YongzokChapter": return "2_1";
            case "DeathChapter": return "3_1";
            case "HeartChapter": return "4_1";
            default: return "";
        }
    }

    // 챕터별 던전 씬 이름 가져오기
    private string GetDungeonSceneForChapter(string chapterId)
    {
        // 챕터별 던전 씬 매핑
        switch (chapterId)
        {
            case "YasuoChapter": return "Chapter1Dungeon";
            case "YongzokChapter": return "Chapter2Dungeon";
            case "DeathChapter": return "Chapter3Dungeon";
            case "HeartChapter": return "Chapter4Dungeon";
            default: return "DungeonScene"; // 기본값
        }
    }
}