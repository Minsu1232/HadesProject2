using System;
using System.Collections.Generic;
using UnityEngine;

public class FragmentChapterUnlocker : MonoBehaviour
{
    public static FragmentChapterUnlocker Instance { get; private set; }

    // 파편 ID와 해금할 챕터 ID 매핑
    [Serializable]
    public class FragmentChapterMapping
    {
        public int fragmentId;
        public string chapterId;
        public string chapterName; // UI 표시용
    }

    [SerializeField]
    private List<FragmentChapterMapping> fragmentChapterMappings = new List<FragmentChapterMapping>()
    {
        new FragmentChapterMapping { fragmentId = 1001, chapterId = "YongzokChapter", chapterName = "용족의 영역" },
        new FragmentChapterMapping { fragmentId = 1002, chapterId = "DeathChapter", chapterName = "죽음의 문" },
        new FragmentChapterMapping { fragmentId = 1003, chapterId = "HeartChapter", chapterName = "심장부" }
    };

    // 액션을 사용한 챕터 해금 이벤트
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
        // 인벤토리 시스템 이벤트 구독
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnItemAdded += CheckFragmentForChapterUnlock;
        }
        else
        {
            Debug.LogError("InventorySystem을 찾을 수 없습니다!");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnItemAdded -= CheckFragmentForChapterUnlock;
        }
    }

    // 아이템 추가될 때 파편인지 확인하고 챕터 해금
    public void CheckFragmentForChapterUnlock(int itemId, int quantity)
    {
        // 이 아이템이 해금 가능한 파편인지 확인
        FragmentChapterMapping mapping = fragmentChapterMappings.Find(m => m.fragmentId == itemId);

        if (mapping != null)
        {
            Debug.Log($"보스 파편 {itemId} 획득 감지! 챕터 해금 시도: {mapping.chapterId}");

            // 챕터 프로그레스 데이터 가져오기
            ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();

            if (chapterData != null)
            {
                // 이미 해금되었는지 확인
                if (!chapterData.IsChapterUnlocked(mapping.chapterId))
                {
                    // 챕터 해금
                    UnlockChapter(mapping.chapterId, mapping.chapterName);
                }
                else
                {
                    Debug.Log($"챕터 {mapping.chapterId}는 이미 해금되어 있습니다.");
                }
            }
            else
            {
                Debug.LogError("ChapterProgressData를 가져올 수 없습니다!");
            }
        }
    }

    // 챕터 해금 처리
    public void UnlockChapter(string chapterId, string chapterName)
    {
        ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();

        if (chapterData != null)
        {
            // 챕터 해금 상태 업데이트
            ChapterProgressData.ChapterData chapter = chapterData.GetChapterData(chapterId);

            if (chapter != null)
            {
                chapter.isUnlocked = true;
                SaveManager.Instance.SaveChapterData();

                Debug.Log($"챕터 해금 성공: {chapterId} ({chapterName})");

                // 이벤트 발생
                OnChapterUnlocked?.Invoke(chapterId, chapterName);

                // UI 알림 표시 (UIManager가 있다면)
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowNotification($"새로운 챕터가 해금되었습니다: {chapterName}",Color.magenta);
                }
            }
            else
            {
                Debug.LogWarning($"챕터 ID {chapterId}에 해당하는 챕터 데이터를 찾을 수 없습니다.");
            }
        }
    }

    // fragmentId에 해당하는 chapterId 반환
    public string GetChapterIdForFragment(int fragmentId)
    {
        FragmentChapterMapping mapping = fragmentChapterMappings.Find(m => m.fragmentId == fragmentId);
        return mapping?.chapterId;
    }

    // chapterId에 해당하는 fragmentId 반환
    public int GetFragmentIdForChapter(string chapterId)
    {
        FragmentChapterMapping mapping = fragmentChapterMappings.Find(m => m.chapterId == chapterId);
        return mapping != null ? mapping.fragmentId : -1;
    }
}