using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class SaveManager : Singleton<SaveManager>
{
    #region �ʵ� �� ���
    private ISaveSystem saveSystem;

    // ������ ���� �̸� ���
    private const string PLAYER_DATA_FILE = "playerData";
    private const string CHAPTER_PROGRESS_FILE = "chapterProgress";
    private const string SETTINGS_FILE = "settings";

    // ���̺� ���� ���� ���
    private const int MAX_SLOTS = 3;
    private int currentSlot = 0;

    // ĳ�̵� ������
    private PlayerSaveData playerData;
    private ChapterProgressData chapterData;
    private GameSettingsData settingsData;

    // ���� ��Ÿ�����͸� ������ Ŭ����
    [Serializable]
    private class SlotMetadata
    {
        public string playerName = "";
        public int chapterProgress = 0;
        public string lastSaveTimeStr = "";
        public int totalPlayTime = 0; // �� ����

        public void SetLastSaveTime(DateTime time)
        {
            lastSaveTimeStr = time.ToString("o"); // ISO 8601 ���� (��: 2024-04-03T15:12:31.1234567Z)
        }

        public DateTime GetLastSaveTime()
        {
            return DateTime.TryParse(lastSaveTimeStr, out var result) ? result : DateTime.MinValue;
        }
    }
    [Serializable]
    private class SlotMetadataWrapper
    {
        public List<SlotMetadata> slots;
    }
    // ��ü ���� ��Ÿ������
    private List<SlotMetadata> slotMetadataList = new List<SlotMetadata>();
    private const string SLOT_METADATA_FILE = "slotMetadata";
    #endregion

    #region �ʱ�ȭ �� �⺻ ������ ����
    protected override void Awake()
    {
        base.Awake();
        saveSystem = new JsonSaveSystem();

        // ���� ��Ÿ������ �ʱ�ȭ (�޸𸮿��� �ε�)
        InitializeSlotMetadata();

        // �⺻ ���� �����ʹ� ��� ������ ����
        LoadSettingsData();

        // ���� ���� ���� Ȯ��
        CheckForBaseStatUpdates();

        // �� ��ü ���� (null ���� ����)
        playerData = new PlayerSaveData();
        chapterData = new ChapterProgressData();
    }

    // �÷��� Ÿ�� ������Ʈ �޼���
    public void UpdatePlayTime(int totalSeconds)
    {
        // ���� ������ ��Ÿ������ ������Ʈ
        if (slotMetadataList != null && currentSlot < slotMetadataList.Count)
        {
            slotMetadataList[currentSlot].totalPlayTime = totalSeconds;
            SaveSlotMetadata();
        }
    }

    // ���� ������ �� �÷��� Ÿ�� ��������
    public int GetTotalPlayTime()
    {
        if (slotMetadataList != null && currentSlot < slotMetadataList.Count)
        {
            return slotMetadataList[currentSlot].totalPlayTime;
        }
        return 0;
    }
    public string GetFormattedPlayTime()
    {
        int totalSeconds = GetTotalPlayTime();

        if (totalSeconds <= 0)
            return "0��";

        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;

        if (hours > 0)
            return $"{hours}�ð� {minutes}��";
        else
            return $"{minutes}��";
    }
    // ���� ��Ÿ������ �ʱ�ȭ
    private void InitializeSlotMetadata()
    {
        // ��Ÿ������ ���� ���
        string metadataPath = Path.Combine(Application.persistentDataPath, "SaveFiles", SLOT_METADATA_FILE + ".json");
        Debug.Log($"��Ÿ������ ���� ����: {File.Exists(metadataPath)}");

        // ��Ÿ������ ������ ������ �ε�
        if (File.Exists(metadataPath))
        {
            try
            {
                string json = File.ReadAllText(metadataPath);
                SlotMetadataWrapper wrapper = JsonUtility.FromJson<SlotMetadataWrapper>(json);

                if (wrapper != null && wrapper.slots != null)
                {
                    slotMetadataList = wrapper.slots;
                    Debug.Log($"���� ��Ÿ������ �ε�: {slotMetadataList.Count}��");
                }
                else
                {
                    Debug.LogError("���� ��Ÿ������ ������ȭ ����");
                    slotMetadataList = new List<SlotMetadata>();
                    CreateDefaultSlotMetadata();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"���� ��Ÿ������ �ε� ����: {e.Message}");
                slotMetadataList = new List<SlotMetadata>();
                CreateDefaultSlotMetadata();
            }
        }
        else
        {
            slotMetadataList = new List<SlotMetadata>();
            CreateDefaultSlotMetadata();
        }

        // ��Ÿ������ ����Ʈ ũ�� Ȯ�� �� ����
        EnsureMetadataListSize();
    }

    // �⺻ ���� ��Ÿ������ ����
    private void CreateDefaultSlotMetadata()
    {
        slotMetadataList = new List<SlotMetadata>();
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            slotMetadataList.Add(new SlotMetadata());
            Debug.Log(slotMetadataList.Count);
        }

        // �κ� �������� ������ �������� ����
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName != "Lobby")
        {
            SaveSlotMetadata();
        }
    }

    // ��Ÿ������ ����Ʈ ũ�� Ȯ�� �� ����
    private void EnsureMetadataListSize()
    {
        // ����Ʈ ũ�Ⱑ MAX_SLOTS���� ������ �׸� �߰�
        while (slotMetadataList.Count < MAX_SLOTS)
        {
            slotMetadataList.Add(new SlotMetadata());
            Debug.Log(slotMetadataList.Count);
        }

        // ����Ʈ ũ�Ⱑ MAX_SLOTS���� ũ�� �ʰ� �׸� ����
        if (slotMetadataList.Count > MAX_SLOTS)
        {
            slotMetadataList.RemoveRange(MAX_SLOTS, slotMetadataList.Count - MAX_SLOTS);
        }
    }

    // ���� ��Ÿ������ ����
    private void SaveSlotMetadata()
    {
        try
        {
            string metadataPath = Path.Combine(Application.persistentDataPath, "SaveFiles", SLOT_METADATA_FILE + ".json");
            string directoryPath = Path.GetDirectoryName(metadataPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // ���� Ŭ������ ����Ͽ� List ����ȭ
            SlotMetadataWrapper wrapper = new SlotMetadataWrapper { slots = slotMetadataList };
            string json = JsonUtility.ToJson(wrapper, true);
            Debug.Log($"��Ÿ������ JSON: {json}"); // ����ȭ�� JSON Ȯ��

            File.WriteAllText(metadataPath, json);
            Debug.Log($"��Ÿ������ ���� ���� �Ϸ�: {metadataPath}");
            // ������ �����Ǿ����� Ȯ��
            if (File.Exists(metadataPath))
            {
                Debug.Log($"��Ÿ������ ���� ���� Ȯ��: ����={new FileInfo(metadataPath).Length}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ��Ÿ������ ���� ����: {e.Message}");
        }
    }

    // ���� ��Ÿ������ ������Ʈ
    private void UpdateCurrentSlotMetadata()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"���� ��: {currentSceneName}");

        if (currentSceneName == "Lobby")
        {
            Debug.LogWarning("�κ� �������� ��Ÿ������ ������Ʈ �ǳʶ�");
            return;
        }
        if (playerData != null && currentSlot < slotMetadataList.Count)
        {
            slotMetadataList[currentSlot].playerName = playerData.userID;
            // ���� é�� ���� ��Ȳ ���
            int maxChapterProgress = 0;
            foreach (var chapter in chapterData.chapters)
            {
                if (chapter.isUnlocked)
                {
                    int chapterNum = ExtractChapterNumber(chapter.chapterId);
                    maxChapterProgress = Mathf.Max(maxChapterProgress, chapterNum);
                }
            }

            // ���� �ִ� é�ͷ� ��Ÿ������ ������Ʈ
            slotMetadataList[currentSlot].chapterProgress = maxChapterProgress;
            slotMetadataList[currentSlot].SetLastSaveTime(DateTime.Now);
            // �� �÷��� Ÿ���� ������ ���� �ʿ�
            Debug.Log($"��Ÿ������ ������Ʈ �õ�: ����={currentSlot}, é��={slotMetadataList[currentSlot].chapterProgress}");
            SaveSlotMetadata();
            Debug.Log("��Ÿ������ ���� �޼��� ȣ�� �Ϸ�");
        }
    }

    // Ȱ�� ���� ����
    public void SetActiveSlot(int slotIndex, bool loadData = true)
    {
        if (slotIndex >= 0 && slotIndex < MAX_SLOTS)
        {
            // ���� �����Ͱ� �ִٸ� ����
            if (playerData != null && playerData.inventory != null && playerData.inventory.Count > 0)
            {
                SaveAllData();
            }

            // ���� ����
            currentSlot = slotIndex;
            ((JsonSaveSystem)saveSystem).SetCurrentSlot(currentSlot);

            // loadData�� true�� ��쿡�� ���� ������ �ε�
            if (loadData)
            {
                LoadSlotData();
            }
            else
            {
                // �޸𸮿� �� ��ü�� ����
                playerData = new PlayerSaveData();
                chapterData = new ChapterProgressData();
            }
        }
        else
        {
            Debug.LogError($"��ȿ���� ���� ���� �ε���: {slotIndex}");
        }
    }

    // ���� ���� �ε��� ��ȯ
    public int GetCurrentSlot()
    {
        return currentSlot;
    }

    // ��� ���� ��Ÿ������ ��ȯ
    public List<SlotMetadataInfo> GetAllSlotMetadata()
    {
        List<SlotMetadataInfo> result = new List<SlotMetadataInfo>();
        int originalSlot = currentSlot;
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            // �ӽ÷� ���� ���� ���� (���� �������� �ʵ��� ����)

            currentSlot = i;
            ((JsonSaveSystem)saveSystem).SetCurrentSlot(i);

            bool hasData = ((JsonSaveSystem)saveSystem).SlotExists();
            SlotMetadataInfo info = new SlotMetadataInfo
            {
                slotIndex = i,
                hasData = hasData,
                playerName = i < slotMetadataList.Count ? slotMetadataList[i].playerName : "",
                chapterProgress = i < slotMetadataList.Count ? slotMetadataList[i].chapterProgress : 0,
                lastSaveTime = i < slotMetadataList.Count ? slotMetadataList[i].GetLastSaveTime() : DateTime.MinValue,
                totalPlayTime = i < slotMetadataList.Count ? slotMetadataList[i].totalPlayTime : 0
            };

            result.Add(info);
        }

        // ���� �������� �ǵ���
        currentSlot = originalSlot;
        ((JsonSaveSystem)saveSystem).SetCurrentSlot(currentSlot);

        return result;
    }

    // ���� ���� ������ �ε�
    private void LoadSlotData()
    {
        try
        {
            LoadPlayerData();
            LoadChapterData();
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ������ �ε� ����: {e.Message}");
            // ���� �� �⺻ �����ͷ� �ʱ�ȭ
            playerData = new PlayerSaveData();
            chapterData = new ChapterProgressData();
        }
    }

    // ���� ������ �ε� (��� ������ ����)
    public void LoadSettingsData()
    {
        try
        {
            string path = GetSettingsPath(SETTINGS_FILE);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                settingsData = JsonUtility.FromJson<GameSettingsData>(json);
            }
            else
            {
                // ���� ������ ������ �⺻�� ����
                settingsData = new GameSettingsData();

                // �κ� �������� ������ �������� ����
                string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (currentSceneName != "Lobby")
                {
                    SaveSettingsData();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"���� �ε� ����: {e.Message}");
            settingsData = new GameSettingsData();
        }
    }

    // �÷��̾� ������ �ε�
    private void LoadPlayerData()
    {
        string playerDataPath = GetSaveFilePath(PLAYER_DATA_FILE);

        // ������ ����, ������ �̹� �����ϸ� �⺻ ������ ����
        if (!File.Exists(playerDataPath) && ((JsonSaveSystem)saveSystem).SlotExists())
        {
            CopyDefaultDataFromStreamingAssets(PLAYER_DATA_FILE);
        }
        // �ƴϸ� �ű� ������ ����
        else if (!File.Exists(playerDataPath))
        {
            CreateNewPlayerData();
            return;
        }

        // ���Ͽ��� �ε�
        playerData = saveSystem.LoadData<PlayerSaveData>(PLAYER_DATA_FILE);

        // playerData�� null�� ��� ó��
        if (playerData == null)
        {
            Debug.LogError("�÷��̾� ������ �ε� ����: �����Ͱ� null�Դϴ�.");
            CreateNewPlayerData();
            return;
        }

        // inventory�� null���� Ȯ��
        if (playerData.inventory == null)
        {
            playerData.inventory = new List<InventoryItemData>();
        }

        // ù ���� �� �⺻ ������ ����
        InitializeDefaultItems();
    }

    // é�� ������ �ε�
    private void LoadChapterData()
    {
        string chapterDataPath = GetSaveFilePath(CHAPTER_PROGRESS_FILE);

        // ������ ����, ������ �̹� �����ϸ� �⺻ ������ ����
        if (!File.Exists(chapterDataPath) && ((JsonSaveSystem)saveSystem).SlotExists())
        {
            CopyDefaultDataFromStreamingAssets(CHAPTER_PROGRESS_FILE);
        }
        // �ƴϸ� �ű� ������ ����
        else if (!File.Exists(chapterDataPath))
        {
            CreateNewChapterData();
            return;
        }

        // ���Ͽ��� �ε�
        chapterData = saveSystem.LoadData<ChapterProgressData>(CHAPTER_PROGRESS_FILE);

        // chapterData�� null�� ��� ó��
        if (chapterData == null)
        {
            Debug.LogError("é�� ������ �ε� ����: �����Ͱ� null�Դϴ�.");
            CreateNewChapterData();
        }
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
        return Path.Combine(Application.persistentDataPath, "SaveFiles", $"Slot{currentSlot}", $"{fileName}.json");
    }

    // ù ���� �� �⺻ ������ �߰�
    private void InitializeDefaultItems()
    {
        // playerData�� null���� Ȯ��
        if (playerData == null)
        {
            Debug.LogError("playerData�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        // inventory�� null���� Ȯ��
        if (playerData.inventory == null)
        {
            playerData.inventory = new List<InventoryItemData>();
        }

        // ó�� ���� �� �⺻ ������ �߰�
        if (playerData.inventory.Count == 0)
        {
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

        // �κ� �������� ������ �������� ����
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName != "Lobby")
        {
            saveSystem.SaveData(playerData, PLAYER_DATA_FILE);
            UpdateCurrentSlotMetadata();
        }

        Debug.Log("�� �÷��̾� �����͸� �����߽��ϴ�.");
    }

    // �� é�� ������ ����
    private void CreateNewChapterData()
    {
        chapterData = new ChapterProgressData();

        // �κ� �������� ������ �������� ����
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName != "Lobby")
        {
            saveSystem.SaveData(chapterData, CHAPTER_PROGRESS_FILE);
        }

        Debug.Log("�� é�� ���� �����͸� �����߽��ϴ�.");
    }

    // �� ���� ������ ����
    private void CreateNewSettingsData()
    {
        settingsData = new GameSettingsData();

        // �κ� �������� ������ �������� ����
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName != "Lobby")
        {
            saveSystem.SaveData(settingsData, SETTINGS_FILE);
        }

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

            // �κ� �������� �������� ����
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentSceneName != "Lobby")
            {
                SaveAllData();
            }
        }
    }
    #endregion

    #region ���� ���
    // �÷��̾� ������ ����
    public void SavePlayerData()
    {
        // �κ� �������� ������ �������� ����
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName == "Lobby")
        {
            return;
        }

        saveSystem.SaveData(playerData, PLAYER_DATA_FILE);

        UpdateCurrentSlotMetadata();
    }

    // é�� ���� ����
    public void SaveChapterData()
    {
        // �κ� �������� ������ �������� ����
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName == "Lobby")
        {
            return;
        }

        saveSystem.SaveData(chapterData, CHAPTER_PROGRESS_FILE);
        UpdateCurrentSlotMetadata();
    }
    private string GetSettingsPath(string fileName) =>
    Path.Combine(Application.persistentDataPath, "SaveFiles", $"{fileName}.json");
    // ���� ����
    public void SaveSettingsData()
    {
        // �κ� �������� ������ �������� ����
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName == "Lobby")
        {
            return;
        }

        // ������ ���� ��ο� ����
        try
        {
            string path = GetSettingsPath(SETTINGS_FILE);
            string directoryPath = Path.GetDirectoryName(path);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string json = JsonUtility.ToJson(settingsData, true);
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ���� ����: {e.Message}");
        }
    }
    // ��� ������ ����
    public void SaveAllData()
    {
        // �κ� �������� ������ �������� ����
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log(currentSceneName);
        if (currentSceneName == "Lobby")
        {
            return;
        }

        SavePlayerData();
        SaveChapterData();
        SaveSettingsData();
        Debug.Log("��� ���� ������ ���� �Ϸ�");
    }

    // ���� ���� �� �ڵ� ����
    private void OnApplicationQuit()
    {
        // �κ� �������� ������ �������� ����
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName != "Lobby")
        {
            SaveAllData();
        }
    }
    #endregion

    #region ���� ���� ���
    // ���� ���� ����
    public void DeleteCurrentSlot()
    {
        ((JsonSaveSystem)saveSystem).DeleteSlot();

        // ��Ÿ������ �ʱ�ȭ
        if (currentSlot < slotMetadataList.Count)
        {
            slotMetadataList[currentSlot] = new SlotMetadata();
            SaveSlotMetadata();
        }

        // �� �����ͷ� �ʱ�ȭ
        playerData = new PlayerSaveData();
        chapterData = new ChapterProgressData();

        Debug.Log($"���� {currentSlot} ���� �Ϸ�");
    }

    // ���� ���Կ� �����Ͱ� �ִ��� Ȯ��
    public bool CurrentSlotHasData()
    {
        return ((JsonSaveSystem)saveSystem).SlotExists();
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

        // PlayerClass �������� (GameInitializer�� ����)
        // ���� ���� ó�� ����� �ٸ��� ��
        PlayerClass playerClass = null;
        if (GameInitializer.Instance != null)
        {
            playerClass = GameInitializer.Instance.GetPlayerClass();
        }

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

        // �÷��̾��� ���� é�� ���� ������Ʈ
        if (completed && playerData != null)
        {
            int numericChapter = ExtractChapterNumber(chapterId);
            if (numericChapter > playerData.currentChapter)
            {
                playerData.currentChapter = numericChapter;
                SavePlayerData();
            }
        }
    }

    // é�� ID���� ���� ���� (��: "YasuoChapter" -> 1)
    private int ExtractChapterNumber(string chapterId)
    {
        if (chapterId == "YasuoChapter") return 1;
        if (chapterId == "YongzokChapter") return 2;
        if (chapterId == "DeathChapter") return 3;
        if (chapterId == "HeartChapter") return 4;
        return 0;
    }
    #endregion

    #region ������ ����
    // ���� �ʱ�ȭ �� ������ ����
    public void ApplyGameData(PlayerClass playerClass, InventorySystem inventory, FragmentManager fragmentManager)
    {
        if (playerClass == null)
        {
            Debug.LogError("PlayerClass�� null�Դϴ�. �����͸� ������ �� �����ϴ�.");
            return;
        }

        // ���� ����
        ApplyStatsToPlayer(playerClass);

        // �κ��丮 ����
        if (inventory != null)
        {
            ApplyInventoryItems(inventory);
        }

        // ���� ����
        if (fragmentManager != null)
        {
            ApplyEquippedFragments(fragmentManager);
        }
    }

    // PlayerClassData�� ���� ����
    public void ApplyStatsToPlayerClassData(PlayerClassData playerClassData)
    {
        if (playerClassData == null) return;
        if (playerData == null)
        {
            Debug.LogError("playerData�� null�Դϴ�. ������ ������ �� �����ϴ�.");
            return;
        }

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
        if (playerData == null)
        {
            Debug.LogError("playerData�� null�Դϴ�. ������ ������ �� �����ϴ�.");
            return;
        }

        PlayerClassData playerClassData = playerClass._playerClassData;
        if (playerClassData == null)
        {
            Debug.LogError("playerClassData�� null�Դϴ�. ������ ������ �� �����ϴ�.");
            return;
        }

        try
        {
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
            if (stats != null)
            {
                stats.UpdateFromPlayerClassData(playerClassData);
                Debug.Log($"�÷��̾� ���� ���� �Ϸ�: HP {stats.MaxHealth}, ���ݷ� {stats.AttackPower}, �ӵ� {stats.Speed}");
            }
            else
            {
                Debug.LogError("PlayerClass.GetStats()�� null�� ��ȯ�߽��ϴ�.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ���� �� ���� �߻�: {e.Message}");
        }
    }

    // �κ��丮 ������ ����
    private void ApplyInventoryItems(InventorySystem inventory)
    {
        if (inventory == null) return;
        if (playerData == null || playerData.inventory == null)
        {
            Debug.LogError("playerData �Ǵ� inventory�� null�Դϴ�. �κ��丮�� ������ �� �����ϴ�.");
            return;
        }

        try
        {
            // �κ��丮 �ʱ�ȭ
            inventory.ClearInventory();

            // ����� ������ �߰� - �÷��� ���纻�� ����Ͽ� ��ȸ
            List<InventoryItemData> itemsToAdd = new List<InventoryItemData>(playerData.inventory);
            foreach (var item in itemsToAdd)
            {
                inventory.AddItem(item.itemID, item.quantity);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"�κ��丮 ���� �� ���� �߻�: {e.Message}");
        }
    }

    // ������ ���� ����
    private void ApplyEquippedFragments(FragmentManager fragmentManager)
    {
        if (fragmentManager == null) return;
        if (playerData == null || playerData.equippedFragments == null)
        {
            Debug.LogError("playerData �Ǵ� equippedFragments�� null�Դϴ�. ������ ������ �� �����ϴ�.");
            return;
        }

        try
        {
            // ���� ���� ����
            fragmentManager.UnequipAllFragments();

            // ����� ���� ����
            foreach (int fragmentId in playerData.equippedFragments)
            {
                FragmentItem fragment = ItemDataManager.Instance?.GetItem(fragmentId) as FragmentItem;
                if (fragment != null)
                {
                    fragmentManager.EquipFragment(fragment);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ���� �� ���� �߻�: {e.Message}");
        }
    }
    #endregion

    #region ��Ÿ ������ ����
    // ����̽� ��� ���� ����
    public void SaveDeviceUnlockStatus(Dictionary<int, bool> deviceStatus)
    {
        if (playerData == null)
        {
            Debug.LogError("playerData�� null�Դϴ�. ����̽� ���¸� ������ �� �����ϴ�.");
            return;
        }

        if (playerData.unlockedDevicesList == null)
        {
            playerData.unlockedDevicesList = new List<DeviceUnlockData>();
        }

        playerData.unlockedDevicesList.Clear();
        foreach (var pair in deviceStatus)
        {
            playerData.unlockedDevicesList.Add(new DeviceUnlockData(pair.Key, pair.Value));
        }
        SavePlayerData();
    }

    // ����̽� ��� ���� ��������
    public Dictionary<int, bool> GetDeviceUnlockStatus()
    {
        Dictionary<int, bool> result = new Dictionary<int, bool>();
        if (playerData?.unlockedDevicesList != null)
        {
            foreach (var data in playerData.unlockedDevicesList)
            {
                result[data.deviceId] = data.isUnlocked;
            }
        }
        return result;
    } 
}

#endregion
