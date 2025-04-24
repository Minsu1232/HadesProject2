using System;
using System.Collections.Generic;
using UnityEngine;

// 캐릭터 스탯 데이터
[System.Serializable]
public class CharacterStatsData
{
    // 참조용 베이스 스탯 (실제로는 사용하지 않음)
    public int baseHp = StatConstants.BASE_HP;
    public int baseGage = StatConstants.BASE_GAGE;
    public int baseAttackPower = StatConstants.BASE_ATTACK_POWER;
    public float baseAttackSpeed = StatConstants.BASE_ATTACK_SPEED;
    public float baseCriticalCance = StatConstants.BASE_CRITICAL_CHANCE;
    public float baseSpeed = StatConstants.BASE_SPEED;
    public float damageReceiveRate = StatConstants.BASE_DAMAGE_RECEIVE_RATE;

    // 업그레이드당 증가량 (참조용)
    public int hpPerUpgrade = StatConstants.HP_PER_UPGRADE;
    public int gagePerUpgrade = StatConstants.GAGE_PER_UPGRADE;
    public int attackPowerPerUpgrade = StatConstants.ATTACK_POWER_PER_UPGRADE;
    public float attackSpeedPerUpgrade = StatConstants.ATTACK_SPEED_PER_UPGRADE;
    public float criticalChancePerUpgrade = StatConstants.CRITICAL_CHANCE_PER_UPGRADE;
    public float speedPerUpgrade = StatConstants.SPEED_PER_UPGRADE;
    public float damageReducePerUpgrade = StatConstants.DAMAGE_REDUCE_PER_UPGRADE;

    // 업그레이드 카운트 (실제로 저장/로드되는 값)
    public int hpUpgradeCount = 0;
    public int gageUpgradeCount = 0;
    public int attackPowerUpgradeCount = 0;
    public int attackSpeedUpgradeCount = 0;
    public int criticalChanceUpgradeCount = 0;
    public int speedUpgradeCount = 0;
    public int damageReduceUpgradeCount = 0;

    // 호환성을 위한 총 업그레이드 카운트
    public int upgradeCount = 0;

    // 모든 업그레이드 카운트의 합계 계산
    public void UpdateTotalUpgradeCount()
    {
        upgradeCount = hpUpgradeCount + gageUpgradeCount + attackPowerUpgradeCount +
                       attackSpeedUpgradeCount + criticalChanceUpgradeCount +
                       speedUpgradeCount + damageReduceUpgradeCount;
    }
}

// 인벤토리 아이템 데이터
[System.Serializable]
public class InventoryItemData
{
    public int itemID;
    public int quantity;

    public InventoryItemData() { }

    public InventoryItemData(int id, int qty)
    {
        itemID = id; 
        quantity = qty;
    }
}
// 플레이어 저장 데이터
[System.Serializable]
public class PlayerSaveData
{
    public string userID = "Player";
    public int currentChapter = 1;
    public List<int> completedQuests = new List<int>();
    public CharacterStatsData characterStats = new CharacterStatsData();
    public List<InventoryItemData> inventory = new List<InventoryItemData>();
    public List<int> equippedFragments = new List<int>();
    public List<DeviceUnlockData> unlockedDevicesList = new List<DeviceUnlockData>();
    

    // 다이얼로그 시스템을 위한 필드 추가
    public List<string> shownDialogs = new List<string>();
    public Dictionary<string, bool> gameFlags = new Dictionary<string, bool>();
    public Dictionary<string, int> locationVisits = new Dictionary<string, int>();
    public List<string> unlockedWeapons = new List<string>();
    public List<string> acquiredFragments = new List<string>();
    public Dictionary<int, AchievementProgress> achievementProgress = new Dictionary<int, AchievementProgress>();
    
    public List<AchievementProgressData> achievementProgressList = new List<AchievementProgressData>();
    public int deathCount = 0; // 죽음 횟수
    public int bossKillCount;
    public int eliteMonsterKillCount;
    public int normalMonsterKillCount;

    // Dictionary -> List 변환 메서드
    public void ConvertDictionaryToList()
    {
        achievementProgressList.Clear();

        if (achievementProgress != null)
        {
            foreach (var pair in achievementProgress)
            {
                achievementProgressList.Add(new AchievementProgressData(pair.Key, pair.Value));
            }
        }
    }

    // List -> Dictionary 변환 메서드
    public void ConvertListToDictionary()
    {
        if (achievementProgress == null)
            achievementProgress = new Dictionary<int, AchievementProgress>();
        else
            achievementProgress.Clear();

        if (achievementProgressList != null)
        {
            foreach (var data in achievementProgressList)
            {
                achievementProgress[data.achievementId] = data.progress;
            }
        }
    }
}
[Serializable]
public class AchievementProgressData
{
    public int achievementId;
    public AchievementProgress progress;

    public AchievementProgressData(int id, AchievementProgress progressData)
    {
        achievementId = id;
        progress = progressData;
    }
}
// 챕터 진행 데이터
[System.Serializable]
public class ChapterProgressData
{
    // 직렬화를 위한 리스트
    public List<ChapterData> chapters = new List<ChapterData>();

    [System.Serializable]
    public class ChapterData
    {
        public string chapterId;
        public bool isUnlocked;
        public string bestRecord = "";
        public int attemptCount = 0;
    }

    // 생성자에서 기본 챕터 초기화
    public ChapterProgressData()
    {
        InitializeDefaultChapters();
    }

    // 기본 챕터 초기화
    private void InitializeDefaultChapters()
    {
        // 이미 초기화되었으면 건너뛰기
        if (chapters.Count > 0)
            return;

        // 기본 챕터 추가 (첫 번째만 해금)
        chapters.Add(new ChapterData { chapterId = "YasuoChapter", isUnlocked = true });
        chapters.Add(new ChapterData { chapterId = "YongzokChapter", isUnlocked = false });
        chapters.Add(new ChapterData { chapterId = "DeathChapter", isUnlocked = false });
        chapters.Add(new ChapterData { chapterId = "HeartChapter", isUnlocked = false });
    }

    // 챕터 데이터 가져오기
    public ChapterData GetChapterData(string chapterId)
    {
        return chapters.Find(c => c.chapterId == chapterId);
    }

    // 챕터 해금 상태 확인
    public bool IsChapterUnlocked(string chapterId)
    {
        ChapterData chapter = GetChapterData(chapterId);
        return chapter != null && chapter.isUnlocked;
    }

    // 챕터 업데이트
    public void UpdateChapter(string chapterId, bool completed, string record = "")
    {
        ChapterData chapter = GetChapterData(chapterId);

        if (chapter != null)
        {
            if (completed)
            {
                chapter.isUnlocked = true;

                // 다음 챕터 해금
                UnlockNextChapter(chapterId);
            }

          
            

            // 기록 업데이트 (비어있거나 더 나은 기록인 경우)
            if (!string.IsNullOrEmpty(record) &&
                (string.IsNullOrEmpty(chapter.bestRecord) ||
                 IsRecordBetter(record, chapter.bestRecord)))
            {
                chapter.bestRecord = record;
            }
        }
    }

    // 다음 챕터 해금
    private void UnlockNextChapter(string chapterId)
    {
        string nextChapterId = "";

        // 다음 챕터 ID 결정
        if (chapterId == "YasuoChapter")
            nextChapterId = "YongzokChapter";
        else if (chapterId == "YongzokChapter")
            nextChapterId = "DeathChapter";
        else if (chapterId == "DeathChapter")
            nextChapterId = "HeartChapter";

        // 다음 챕터 해금
        if (!string.IsNullOrEmpty(nextChapterId))
        {
            ChapterData nextChapter = GetChapterData(nextChapterId);
            if (nextChapter != null)
                nextChapter.isUnlocked = true;
        }
    }

    // 기록 비교
    private bool IsRecordBetter(string newRecord, string oldRecord)
    {
        // 기록 비교 로직
        int newValue = ExtractNumberFromRecord(newRecord);
        int oldValue = ExtractNumberFromRecord(oldRecord);
        return newValue > oldValue;
    }

    // 기록에서 숫자 추출 (예: "1-10"에서 10 추출)
    private int ExtractNumberFromRecord(string record)
    {
        string[] parts = record.Split('-');
        if (parts.Length > 1 && int.TryParse(parts[1], out int result))
            return result;
        return 0;
    }

    // 챕터 시도 횟수 가져오기
    public int GetChapterAttempts(string chapterId)
    {
        ChapterData chapter = GetChapterData(chapterId);
        return chapter?.attemptCount ?? 0;
    }

    // 챕터 최고 기록 가져오기
    public string GetChapterBestRecord(string chapterId)
    {
        ChapterData chapter = GetChapterData(chapterId);
        return chapter?.bestRecord ?? "";
    }
}
[Serializable]
public class DeviceUnlockData
{
    public int deviceId;
    public bool isUnlocked;

    public DeviceUnlockData(int id, bool unlocked)
    {
        deviceId = id;
        isUnlocked = unlocked;
    }
}
// 게임 설정 데이터
[System.Serializable]
public class GameSettingsData
{
    // 오디오 설정
    public float musicVolume = 0.5f;

    // 화면 설정
    public bool fullscreen = true;

    // 언어 설정
    public string language = "Korean"; // "Korean" 또는 "English"
}