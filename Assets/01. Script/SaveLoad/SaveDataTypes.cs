using System;
using System.Collections.Generic;
using UnityEngine;

// ĳ���� ���� ������
[System.Serializable]
public class CharacterStatsData
{
    // ������ ���̽� ���� (�����δ� ������� ����)
    public int baseHp = StatConstants.BASE_HP;
    public int baseGage = StatConstants.BASE_GAGE;
    public int baseAttackPower = StatConstants.BASE_ATTACK_POWER;
    public float baseAttackSpeed = StatConstants.BASE_ATTACK_SPEED;
    public float baseCriticalCance = StatConstants.BASE_CRITICAL_CHANCE;
    public float baseSpeed = StatConstants.BASE_SPEED;
    public float damageReceiveRate = StatConstants.BASE_DAMAGE_RECEIVE_RATE;

    // ���׷��̵�� ������ (������)
    public int hpPerUpgrade = StatConstants.HP_PER_UPGRADE;
    public int gagePerUpgrade = StatConstants.GAGE_PER_UPGRADE;
    public int attackPowerPerUpgrade = StatConstants.ATTACK_POWER_PER_UPGRADE;
    public float attackSpeedPerUpgrade = StatConstants.ATTACK_SPEED_PER_UPGRADE;
    public float criticalChancePerUpgrade = StatConstants.CRITICAL_CHANCE_PER_UPGRADE;
    public float speedPerUpgrade = StatConstants.SPEED_PER_UPGRADE;
    public float damageReducePerUpgrade = StatConstants.DAMAGE_REDUCE_PER_UPGRADE;

    // ���׷��̵� ī��Ʈ (������ ����/�ε�Ǵ� ��)
    public int hpUpgradeCount = 0;
    public int gageUpgradeCount = 0;
    public int attackPowerUpgradeCount = 0;
    public int attackSpeedUpgradeCount = 0;
    public int criticalChanceUpgradeCount = 0;
    public int speedUpgradeCount = 0;
    public int damageReduceUpgradeCount = 0;

    // ȣȯ���� ���� �� ���׷��̵� ī��Ʈ
    public int upgradeCount = 0;

    // ��� ���׷��̵� ī��Ʈ�� �հ� ���
    public void UpdateTotalUpgradeCount()
    {
        upgradeCount = hpUpgradeCount + gageUpgradeCount + attackPowerUpgradeCount +
                       attackSpeedUpgradeCount + criticalChanceUpgradeCount +
                       speedUpgradeCount + damageReduceUpgradeCount;
    }
}

// �κ��丮 ������ ������
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
// �÷��̾� ���� ������
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
    

    // ���̾�α� �ý����� ���� �ʵ� �߰�
    public List<string> shownDialogs = new List<string>();
    public Dictionary<string, bool> gameFlags = new Dictionary<string, bool>();
    public Dictionary<string, int> locationVisits = new Dictionary<string, int>();
    public List<string> unlockedWeapons = new List<string>();
    public List<string> acquiredFragments = new List<string>();
    public Dictionary<int, AchievementProgress> achievementProgress = new Dictionary<int, AchievementProgress>();
    
    public List<AchievementProgressData> achievementProgressList = new List<AchievementProgressData>();
    public int deathCount = 0; // ���� Ƚ��
    public int bossKillCount;
    public int eliteMonsterKillCount;
    public int normalMonsterKillCount;

    // Dictionary -> List ��ȯ �޼���
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

    // List -> Dictionary ��ȯ �޼���
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
// é�� ���� ������
[System.Serializable]
public class ChapterProgressData
{
    // ����ȭ�� ���� ����Ʈ
    public List<ChapterData> chapters = new List<ChapterData>();

    [System.Serializable]
    public class ChapterData
    {
        public string chapterId;
        public bool isUnlocked;
        public string bestRecord = "";
        public int attemptCount = 0;
    }

    // �����ڿ��� �⺻ é�� �ʱ�ȭ
    public ChapterProgressData()
    {
        InitializeDefaultChapters();
    }

    // �⺻ é�� �ʱ�ȭ
    private void InitializeDefaultChapters()
    {
        // �̹� �ʱ�ȭ�Ǿ����� �ǳʶٱ�
        if (chapters.Count > 0)
            return;

        // �⺻ é�� �߰� (ù ��°�� �ر�)
        chapters.Add(new ChapterData { chapterId = "YasuoChapter", isUnlocked = true });
        chapters.Add(new ChapterData { chapterId = "YongzokChapter", isUnlocked = false });
        chapters.Add(new ChapterData { chapterId = "DeathChapter", isUnlocked = false });
        chapters.Add(new ChapterData { chapterId = "HeartChapter", isUnlocked = false });
    }

    // é�� ������ ��������
    public ChapterData GetChapterData(string chapterId)
    {
        return chapters.Find(c => c.chapterId == chapterId);
    }

    // é�� �ر� ���� Ȯ��
    public bool IsChapterUnlocked(string chapterId)
    {
        ChapterData chapter = GetChapterData(chapterId);
        return chapter != null && chapter.isUnlocked;
    }

    // é�� ������Ʈ
    public void UpdateChapter(string chapterId, bool completed, string record = "")
    {
        ChapterData chapter = GetChapterData(chapterId);

        if (chapter != null)
        {
            if (completed)
            {
                chapter.isUnlocked = true;

                // ���� é�� �ر�
                UnlockNextChapter(chapterId);
            }

          
            

            // ��� ������Ʈ (����ְų� �� ���� ����� ���)
            if (!string.IsNullOrEmpty(record) &&
                (string.IsNullOrEmpty(chapter.bestRecord) ||
                 IsRecordBetter(record, chapter.bestRecord)))
            {
                chapter.bestRecord = record;
            }
        }
    }

    // ���� é�� �ر�
    private void UnlockNextChapter(string chapterId)
    {
        string nextChapterId = "";

        // ���� é�� ID ����
        if (chapterId == "YasuoChapter")
            nextChapterId = "YongzokChapter";
        else if (chapterId == "YongzokChapter")
            nextChapterId = "DeathChapter";
        else if (chapterId == "DeathChapter")
            nextChapterId = "HeartChapter";

        // ���� é�� �ر�
        if (!string.IsNullOrEmpty(nextChapterId))
        {
            ChapterData nextChapter = GetChapterData(nextChapterId);
            if (nextChapter != null)
                nextChapter.isUnlocked = true;
        }
    }

    // ��� ��
    private bool IsRecordBetter(string newRecord, string oldRecord)
    {
        // ��� �� ����
        int newValue = ExtractNumberFromRecord(newRecord);
        int oldValue = ExtractNumberFromRecord(oldRecord);
        return newValue > oldValue;
    }

    // ��Ͽ��� ���� ���� (��: "1-10"���� 10 ����)
    private int ExtractNumberFromRecord(string record)
    {
        string[] parts = record.Split('-');
        if (parts.Length > 1 && int.TryParse(parts[1], out int result))
            return result;
        return 0;
    }

    // é�� �õ� Ƚ�� ��������
    public int GetChapterAttempts(string chapterId)
    {
        ChapterData chapter = GetChapterData(chapterId);
        return chapter?.attemptCount ?? 0;
    }

    // é�� �ְ� ��� ��������
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
// ���� ���� ������
[System.Serializable]
public class GameSettingsData
{
    // ����� ����
    public float musicVolume = 0.5f;

    // ȭ�� ����
    public bool fullscreen = true;

    // ��� ����
    public string language = "Korean"; // "Korean" �Ǵ� "English"
}