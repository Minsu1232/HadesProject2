//using System.Collections.Generic;
//using UnityEngine;

//// 챕터 진행 데이터
//[System.Serializable]
//public class ChapterProgressData
//{
//    // 내부적으로 사용할 Dictionary
//    [System.NonSerialized]
//    public Dictionary<string, bool> unlockedChapters = new Dictionary<string, bool>();
//    [System.NonSerialized]
//    public Dictionary<string, string> bestRecords = new Dictionary<string, string>();
//    [System.NonSerialized]
//    public Dictionary<string, int> attemptCounts = new Dictionary<string, int>();

//    // 직렬화를 위한 리스트
//    [SerializeField]
//    public List<ChapterUnlockEntry> chapterUnlockList = new List<ChapterUnlockEntry>();
//    [SerializeField]
//    public List<ChapterRecordEntry> chapterRecordList = new List<ChapterRecordEntry>();
//    [SerializeField]
//    public List<ChapterAttemptEntry> chapterAttemptList = new List<ChapterAttemptEntry>();

//    // 시리얼라이즈 가능한 내부 클래스들
//    [System.Serializable]
//    public class ChapterUnlockEntry
//    {
//        public string chapterId;
//        public bool isUnlocked;
//    }

//    [System.Serializable]
//    public class ChapterRecordEntry
//    {
//        public string chapterId;
//        public string record;
//    }

//    [System.Serializable]
//    public class ChapterAttemptEntry
//    {
//        public string chapterId;
//        public int count;
//    }

//    // 생성자
//    public ChapterProgressData()
//    {
//        InitializeDefaultData();
//    }

//    // 기본 데이터 초기화
//    private void InitializeDefaultData()
//    {
//        // 기본 챕터 설정 (첫 번째만 해금)
//        unlockedChapters["YasuoChapter"] = true;
//        unlockedChapters["YongzokChapter"] = true;
//        unlockedChapters["DeathChapter"] = false;
//        unlockedChapters["HeartChapter"] = false;

//        bestRecords["YasuoChapter"] = "";
//        bestRecords["YongzokChapter"] = "";
//        bestRecords["DeathChapter"] = "";
//        bestRecords["HeartChapter"] = "";

//        attemptCounts["YasuoChapter"] = 0;
//        attemptCounts["YongzokChapter"] = 0;
//        attemptCounts["DeathChapter"] = 0;
//        attemptCounts["HeartChapter"] = 0;

//        // 리스트 초기화
//        ConvertDictionariesToLists();
//    }

//    // Dictionary를 List로 변환 (저장 전 호출)
//    public void ConvertDictionariesToLists()
//    {
//        chapterUnlockList.Clear();
//        chapterRecordList.Clear();
//        chapterAttemptList.Clear();

//        foreach (var entry in unlockedChapters)
//        {
//            chapterUnlockList.Add(new ChapterUnlockEntry
//            {
//                chapterId = entry.Key,
//                isUnlocked = entry.Value
//            });
//        }

//        foreach (var entry in bestRecords)
//        {
//            chapterRecordList.Add(new ChapterRecordEntry
//            {
//                chapterId = entry.Key,
//                record = entry.Value
//            });
//        }

//        foreach (var entry in attemptCounts)
//        {
//            chapterAttemptList.Add(new ChapterAttemptEntry
//            {
//                chapterId = entry.Key,
//                count = entry.Value
//            });
//        }
//    }

//    // List를 Dictionary로 변환 (로드 후 호출)
//    public void ConvertListsToDictionaries()
//    {
//        unlockedChapters.Clear();
//        bestRecords.Clear();
//        attemptCounts.Clear();

//        foreach (var entry in chapterUnlockList)
//        {
//            unlockedChapters[entry.chapterId] = entry.isUnlocked;
//        }

//        foreach (var entry in chapterRecordList)
//        {
//            bestRecords[entry.chapterId] = entry.record;
//        }

//        foreach (var entry in chapterAttemptList)
//        {
//            attemptCounts[entry.chapterId] = entry.count;
//        }
//    }

//    // 챕터 업데이트 메서드
//    public void UpdateChapter(int chapterIndex, bool completed)
//    {
//        string chapterId = GetChapterIdByIndex(chapterIndex);

//        if (!string.IsNullOrEmpty(chapterId))
//        {
//            if (completed)
//            {
//                unlockedChapters[chapterId] = true;

//                // 다음 챕터 해금
//                string nextChapterId = GetChapterIdByIndex(chapterIndex + 1);
//                if (!string.IsNullOrEmpty(nextChapterId))
//                {
//                    unlockedChapters[nextChapterId] = true;
//                }
//            }

//            // Dictionary를 List로 변환 (저장 준비)
//            ConvertDictionariesToLists();
//        }
//    }

//    // 챕터 인덱스로 ID 가져오기
//    private string GetChapterIdByIndex(int index)
//    {
//        switch (index)
//        {
//            case 1: return "YasuoChapter";
//            case 2: return "YongzokChapter";
//            case 3: return "DeathChapter";
//            case 4: return "HeartChapter";
//            default: return "";
//        }
//    }

//    // 챕터 시도 횟수 증가
//    public void IncrementChapterAttempt(string chapterId)
//    {
//        if (attemptCounts.ContainsKey(chapterId))
//        {
//            attemptCounts[chapterId]++;
//            ConvertDictionariesToLists();
//        }
//    }

//    // 챕터 기록 업데이트
//    public void UpdateChapterRecord(string chapterId, string record)
//    {
//        if (bestRecords.ContainsKey(chapterId))
//        {
//            bestRecords[chapterId] = record;
//            ConvertDictionariesToLists();
//        }
//    }
//}