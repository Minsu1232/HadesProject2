using System.IO;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    #region �ʵ� �� ���
    private ISaveSystem saveSystem;

    // ������ ���� �̸� ���
    private const string PLAYER_DATA_FILE = "playerData";
    private const string CHAPTER_PROGRESS_FILE = "chapterProgress";
    private const string SETTINGS_FILE = "settings";

    // ĳ�̵� ������
    private PlayerSaveData playerData;
    private ChapterProgressData chapterData;
    private GameSettingsData settingsData;
    #endregion

    #region �ʱ�ȭ �� �⺻ ������ ����
    private void Awake()
    {
        saveSystem = new JsonSaveSystem();
        LoadAllData();
        CheckForBaseStatUpdates();
    }

    // ��� ������ �ε�
    private void LoadAllData()
    {
        // ���� ���� ��� Ȯ��
        string playerDataPath = GetSaveFilePath(PLAYER_DATA_FILE);
        string chapterDataPath = GetSaveFilePath(CHAPTER_PROGRESS_FILE);
        string settingsDataPath = GetSaveFilePath(SETTINGS_FILE);

        // ���� ������ ������ ��Ʈ���� ���¿��� ����
        if (!File.Exists(playerDataPath))
        {
            CopyDefaultDataFromStreamingAssets(PLAYER_DATA_FILE);
        }

        if (!File.Exists(chapterDataPath))
        {
            CopyDefaultDataFromStreamingAssets(CHAPTER_PROGRESS_FILE);
        }

        if (!File.Exists(settingsDataPath))
        {
            CopyDefaultDataFromStreamingAssets(SETTINGS_FILE);
        }

        // ���� �ε�
        playerData = saveSystem.LoadData<PlayerSaveData>(PLAYER_DATA_FILE);
        chapterData = saveSystem.LoadData<ChapterProgressData>(CHAPTER_PROGRESS_FILE);
        settingsData = saveSystem.LoadData<GameSettingsData>(SETTINGS_FILE);

        // ù ���� �� �⺻ ������ ����
        InitializeDefaultItems();
    }

    // ��Ʈ���� ���¿��� �⺻ ������ ���� �޼���
    private void CopyDefaultDataFromStreamingAssets(string fileName)
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "DefaultSave", fileName + ".json");
        string destPath = GetSaveFilePath(fileName);

        // ��Ʈ���� ���¿� ������ �ִ��� Ȯ��
        if (File.Exists(sourcePath))
        {
            // ���� ���丮�� ������ ����
            string destDir = Path.GetDirectoryName(destPath);
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // ���� ����
            File.Copy(sourcePath, destPath);
            Debug.Log($"�⺻ ������ ������ �����߽��ϴ�: {fileName}");
        }
        else
        {
            // ��Ʈ���� ���¿� ������ ������ �⺻ ��ü ����
            if (fileName == PLAYER_DATA_FILE)
                CreateNewPlayerData();
            else if (fileName == CHAPTER_PROGRESS_FILE)
                CreateNewChapterData();
            else if (fileName == SETTINGS_FILE)
                CreateNewSettingsData();
        }
    }

    // ���� ���� ��� ��������
    private string GetSaveFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, "SaveFiles", $"{fileName}.json");
    }

    // ù ���� �� �⺻ ������ �߰�
    private void InitializeDefaultItems()
    {
        // ó�� ���� �� �⺻ ������ �߰�
        if (playerData.inventory.Count == 0)
        {
            playerData.inventory.Add(new InventoryItemData(3001, 3)); // ü�� ����
            playerData.inventory.Add(new InventoryItemData(3002, 1)); // �⺻ ���
            SavePlayerData();
        }
    }

    // �� �÷��̾� ������ ����
    private void CreateNewPlayerData()
    {
        playerData = new PlayerSaveData();

        // ���̽� ������ StatConstants ���� ��ġ��Ŵ (������)
        playerData.characterStats.baseHp = StatConstants.BASE_HP;
        playerData.characterStats.baseGage = StatConstants.BASE_GAGE;
        playerData.characterStats.baseAttackPower = StatConstants.BASE_ATTACK_POWER;
        playerData.characterStats.baseAttackSpeed = StatConstants.BASE_ATTACK_SPEED;
        playerData.characterStats.baseCriticalCance = StatConstants.BASE_CRITICAL_CHANCE;
        playerData.characterStats.baseSpeed = StatConstants.BASE_SPEED;
        playerData.characterStats.damageReceiveRate = StatConstants.BASE_DAMAGE_RECEIVE_RATE;

        // ���׷��̵�� �������� ��ġ��Ŵ (������)
        playerData.characterStats.hpPerUpgrade = StatConstants.HP_PER_UPGRADE;
        playerData.characterStats.gagePerUpgrade = StatConstants.GAGE_PER_UPGRADE;
        playerData.characterStats.attackPowerPerUpgrade = StatConstants.ATTACK_POWER_PER_UPGRADE;
        playerData.characterStats.attackSpeedPerUpgrade = StatConstants.ATTACK_SPEED_PER_UPGRADE;
        playerData.characterStats.criticalChancePerUpgrade = StatConstants.CRITICAL_CHANCE_PER_UPGRADE;
        playerData.characterStats.speedPerUpgrade = StatConstants.SPEED_PER_UPGRADE;
        playerData.characterStats.damageReducePerUpgrade = StatConstants.DAMAGE_REDUCE_PER_UPGRADE;

        // ���׷��̵� ī��Ʈ �ʱ�ȭ
        playerData.characterStats.hpUpgradeCount = 0;
        playerData.characterStats.gageUpgradeCount = 0;
        playerData.characterStats.attackPowerUpgradeCount = 0;
        playerData.characterStats.attackSpeedUpgradeCount = 0;
        playerData.characterStats.criticalChanceUpgradeCount = 0;
        playerData.characterStats.speedUpgradeCount = 0;
        playerData.characterStats.damageReduceUpgradeCount = 0;
        playerData.characterStats.UpdateTotalUpgradeCount();

        saveSystem.SaveData(playerData, PLAYER_DATA_FILE);
        Debug.Log("�� �÷��̾� �����͸� �����߽��ϴ�.");
    }

    // �� é�� ������ ����
    private void CreateNewChapterData()
    {
        chapterData = new ChapterProgressData();
        saveSystem.SaveData(chapterData, CHAPTER_PROGRESS_FILE);
        Debug.Log("�� é�� ���� �����͸� �����߽��ϴ�.");
    }

    // �� ���� ������ ����
    private void CreateNewSettingsData()
    {
        settingsData = new GameSettingsData();
        saveSystem.SaveData(settingsData, SETTINGS_FILE);
        Debug.Log("�� ���� �����͸� �����߽��ϴ�.");
    }

    // ���� ������ ���� ���̽� ���� ������Ʈ Ȯ��
    private void CheckForBaseStatUpdates()
    {
        // ���� ���� ���� Ȯ�� (PlayerPrefs�� ����� ������ ��)
        string currentVersion = Application.version;
        string savedVersion = PlayerPrefs.GetString("GameVersion", "");

        if (currentVersion != savedVersion)
        {
            Debug.Log($"���� ���� ���� ����: {savedVersion} -> {currentVersion}");

            // ���⿡ ������Ʈ ���� �߰�
            // ������Ʈ �� ���� ������ �ʿ��� ���:
            // ���̽� ������ �ڵ忡 ���� ���ǵǾ� �����Ƿ� ���� ���� �ʿ� ����
            // �ʿ��ϴٸ� ���׷��̵� ī��Ʈ�� ȿ���� �����ϴ� ���� �߰� ����

            // ���� ���� ������Ʈ
            PlayerPrefs.SetString("GameVersion", currentVersion);
            SaveAllData();
        }
    }
    #endregion

    #region ���� ���
    // �÷��̾� ������ ����
    public void SavePlayerData()
    {
        saveSystem.SaveData(playerData, PLAYER_DATA_FILE);
    }

    // é�� ���� ����
    public void SaveChapterData()
    {
        saveSystem.SaveData(chapterData, CHAPTER_PROGRESS_FILE);
    }

    // ���� ����
    public void SaveSettingsData()
    {
        saveSystem.SaveData(settingsData, SETTINGS_FILE);
    }

    // ��� ������ ����
    public void SaveAllData()
    {
        SavePlayerData();
        SaveChapterData();
        SaveSettingsData();
        Debug.Log("��� ���� ������ ���� �Ϸ�");
    }

    // ���� ���� �� �ڵ� ����
    private void OnApplicationQuit()
    {
        SaveAllData();
    }
    #endregion

    #region ������ ������
    // ������ ������
    public PlayerSaveData GetPlayerData() => playerData;
    public ChapterProgressData GetChapterData() => chapterData;
    public GameSettingsData GetSettingsData() => settingsData;
    #endregion

    #region ������ ������Ʈ
    // �÷��̾� ���� ������Ʈ
    public void UpdatePlayerStats(Stats gameStats)
    {
        if (gameStats == null) return;

        PlayerClass playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass == null) return;

        // ���� ���׷��̵� ī��Ʈ�� ����
        playerData.characterStats.hpUpgradeCount = playerClass._playerClassData.characterStats.hpUpgradeCount;
        playerData.characterStats.gageUpgradeCount = playerClass._playerClassData.characterStats.gageUpgradeCount;
        playerData.characterStats.attackPowerUpgradeCount = playerClass._playerClassData.characterStats.attackPowerUpgradeCount;
        playerData.characterStats.attackSpeedUpgradeCount = playerClass._playerClassData.characterStats.attackSpeedUpgradeCount;
        playerData.characterStats.criticalChanceUpgradeCount = playerClass._playerClassData.characterStats.criticalChanceUpgradeCount;
        playerData.characterStats.speedUpgradeCount = playerClass._playerClassData.characterStats.speedUpgradeCount;
        playerData.characterStats.damageReduceUpgradeCount = playerClass._playerClassData.characterStats.damageReduceUpgradeCount;

        // �� ���׷��̵� ī��Ʈ ������Ʈ
        playerData.characterStats.UpdateTotalUpgradeCount();

        SavePlayerData();
    }

    // �κ��丮 ������Ʈ
    public void UpdateInventory(InventorySystem inventory)
    {
        if (inventory == null) return;

        playerData.inventory.Clear();

        foreach (var slot in inventory.GetAllItems())
        {
            playerData.inventory.Add(new InventoryItemData(
                slot.item.itemID,
                slot.quantity
            ));
        }

        SavePlayerData();
    }

    // ������ ���� ������Ʈ
    public void UpdateEquippedFragments(FragmentManager fragmentManager)
    {
        if (fragmentManager == null) return;

        playerData.equippedFragments.Clear();

        foreach (var fragment in fragmentManager.GetEquippedFragments())
        {
            playerData.equippedFragments.Add(fragment.itemID);
        }

        SavePlayerData();
    }

    // é�� ���� ������Ʈ
    public void UpdateChapterProgress(string chapterId, bool completed, string record = "")
    {
        chapterData.UpdateChapter(chapterId, completed, record);
        SaveChapterData();
    }
    #endregion

    #region ������ ����
    // ���� �ʱ�ȭ �� ������ ����
    public void ApplyGameData(PlayerClass playerClass, InventorySystem inventory, FragmentManager fragmentManager)
    {
        // ���� ����
        ApplyStatsToPlayer(playerClass);

        // �κ��丮 ����
        ApplyInventoryItems(inventory);

        // ���� ����
        ApplyEquippedFragments(fragmentManager);
    }

    // PlayerClassData�� ���� ����
    public void ApplyStatsToPlayerClassData(PlayerClassData playerClassData)
    {
        if (playerClassData == null) return;

        playerClassData.userID = playerData.userID;
        playerClassData.currentChapter = playerData.currentChapter;

        // ���׷��̵� ī��Ʈ ���� (���̽� ������ �������� ����)
        playerClassData.characterStats.hpUpgradeCount = playerData.characterStats.hpUpgradeCount;
        playerClassData.characterStats.gageUpgradeCount = playerData.characterStats.gageUpgradeCount;
        playerClassData.characterStats.attackPowerUpgradeCount = playerData.characterStats.attackPowerUpgradeCount;
        playerClassData.characterStats.attackSpeedUpgradeCount = playerData.characterStats.attackSpeedUpgradeCount;
        playerClassData.characterStats.criticalChanceUpgradeCount = playerData.characterStats.criticalChanceUpgradeCount;
        playerClassData.characterStats.speedUpgradeCount = playerData.characterStats.speedUpgradeCount;
        playerClassData.characterStats.damageReduceUpgradeCount = playerData.characterStats.damageReduceUpgradeCount;

        // �� ���׷��̵� ī��Ʈ ������Ʈ
        playerClassData.characterStats.UpdateTotalUpgradeCount();
    }

    // PlayerClass�� ���� ����
    private void ApplyStatsToPlayer(PlayerClass playerClass)
    {
        if (playerClass == null) return;

        PlayerClassData playerClassData = playerClass._playerClassData;

        // ���׷��̵� ī��Ʈ ����
        playerClassData.characterStats.hpUpgradeCount = playerData.characterStats.hpUpgradeCount;
        playerClassData.characterStats.gageUpgradeCount = playerData.characterStats.gageUpgradeCount;
        playerClassData.characterStats.attackPowerUpgradeCount = playerData.characterStats.attackPowerUpgradeCount;
        playerClassData.characterStats.attackSpeedUpgradeCount = playerData.characterStats.attackSpeedUpgradeCount;
        playerClassData.characterStats.criticalChanceUpgradeCount = playerData.characterStats.criticalChanceUpgradeCount;
        playerClassData.characterStats.speedUpgradeCount = playerData.characterStats.speedUpgradeCount;
        playerClassData.characterStats.damageReduceUpgradeCount = playerData.characterStats.damageReduceUpgradeCount;

        // �� ���׷��̵� ī��Ʈ ������Ʈ
        playerClassData.characterStats.UpdateTotalUpgradeCount();

        // ��Ÿ �÷��̾� ������ ����
        playerClassData.userID = playerData.userID;
        playerClassData.currentChapter = playerData.currentChapter;

        // ����Ʈ ������ ����
        playerClassData.completedQuests.Clear();
        foreach (int questId in playerData.completedQuests)
        {
            playerClassData.completedQuests.Add(questId);
        }

        // �κ��丮 ������ ����
        playerClassData.inventory.Clear();
        foreach (var item in playerData.inventory)
        {
            PlayerClassData.InventoryItem newItem = new PlayerClassData.InventoryItem
            {
                itemID = item.itemID,
                quantity = item.quantity
            };
            playerClassData.inventory.Add(newItem);
        }

        // ���� ���� ���� ������ Stats ������Ʈ
        Stats stats = playerClass.GetStats();
        stats.UpdateFromPlayerClassData(playerClassData);

        // ����� �α�
        Debug.Log($"�÷��̾� ���� ���� �Ϸ�: HP {stats.MaxHealth}, ���ݷ� {stats.AttackPower}, �ӵ� {stats.Speed}");
    }

    // �κ��丮 ������ ����
    private void ApplyInventoryItems(InventorySystem inventory)
    {
        if (inventory == null) return;

        // �κ��丮 �ʱ�ȭ
        inventory.ClearInventory();

        // ����� ������ �߰�
        foreach (var item in playerData.inventory)
        {
            inventory.AddItem(item.itemID, item.quantity);
        }
    }

    // ������ ���� ����
    private void ApplyEquippedFragments(FragmentManager fragmentManager)
    {
        if (fragmentManager == null) return;

        // ���� ���� ����
        fragmentManager.UnequipAllFragments();

        // ����� ���� ����
        foreach (int fragmentId in playerData.equippedFragments)
        {
            FragmentItem fragment = ItemDataManager.Instance.GetItem(fragmentId) as FragmentItem;
            if (fragment != null)
            {
                fragmentManager.EquipFragment(fragment);
            }
        }
    }
    #endregion
}