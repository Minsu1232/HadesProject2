using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class SaveManager : Singleton<SaveManager>
{
    #region 필드 및 상수
    private ISaveSystem saveSystem;

    // 데이터 파일 이름 상수
    private const string PLAYER_DATA_FILE = "playerData";
    private const string CHAPTER_PROGRESS_FILE = "chapterProgress";
    private const string SETTINGS_FILE = "settings";

    // 세이브 슬롯 관련 상수
    private const int MAX_SLOTS = 3;
    private int currentSlot = 0;

    // 캐싱된 데이터
    private PlayerSaveData playerData;
    private ChapterProgressData chapterData;
    private GameSettingsData settingsData;

    // 슬롯 메타데이터를 저장할 클래스
    [Serializable]
    private class SlotMetadata
    {
        public string playerName = "";
        public int chapterProgress = 0;
        public string lastSaveTimeStr = "";
        public int totalPlayTime = 0; // 초 단위

        public void SetLastSaveTime(DateTime time)
        {
            lastSaveTimeStr = time.ToString("o"); // ISO 8601 포맷 (예: 2024-04-03T15:12:31.1234567Z)
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
    // 전체 슬롯 메타데이터
    private List<SlotMetadata> slotMetadataList = new List<SlotMetadata>();
    private const string SLOT_METADATA_FILE = "slotMetadata";
    #endregion

    #region 초기화 및 기본 데이터 관리
    protected override void Awake()
    {
        base.Awake();
        saveSystem = new JsonSaveSystem();

        // 슬롯 메타데이터 초기화 (메모리에만 로드)
        InitializeSlotMetadata();

        // 기본 설정 데이터는 모든 슬롯이 공유
        LoadSettingsData();

        // 현재 게임 버전 확인
        CheckForBaseStatUpdates();

        // 빈 객체 생성 (null 참조 방지)
        playerData = new PlayerSaveData();
        chapterData = new ChapterProgressData();
    }

    // 플레이 타임 업데이트 메서드
    public void UpdatePlayTime(int totalSeconds)
    {
        // 현재 슬롯의 메타데이터 업데이트
        if (slotMetadataList != null && currentSlot < slotMetadataList.Count)
        {
            slotMetadataList[currentSlot].totalPlayTime = totalSeconds;
            SaveSlotMetadata();
        }
    }

    // 현재 슬롯의 총 플레이 타임 가져오기
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
            return "0분";

        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;

        if (hours > 0)
            return $"{hours}시간 {minutes}분";
        else
            return $"{minutes}분";
    }
    // 슬롯 메타데이터 초기화
    private void InitializeSlotMetadata()
    {
        // 메타데이터 폴더 경로
        string metadataPath = Path.Combine(Application.persistentDataPath, "SaveFiles", SLOT_METADATA_FILE + ".json");
        Debug.Log($"메타데이터 파일 존재: {File.Exists(metadataPath)}");

        // 메타데이터 파일이 있으면 로드
        if (File.Exists(metadataPath))
        {
            try
            {
                string json = File.ReadAllText(metadataPath);
                SlotMetadataWrapper wrapper = JsonUtility.FromJson<SlotMetadataWrapper>(json);

                if (wrapper != null && wrapper.slots != null)
                {
                    slotMetadataList = wrapper.slots;
                    Debug.Log($"슬롯 메타데이터 로드: {slotMetadataList.Count}개");
                }
                else
                {
                    Debug.LogError("슬롯 메타데이터 역직렬화 실패");
                    slotMetadataList = new List<SlotMetadata>();
                    CreateDefaultSlotMetadata();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"슬롯 메타데이터 로드 실패: {e.Message}");
                slotMetadataList = new List<SlotMetadata>();
                CreateDefaultSlotMetadata();
            }
        }
        else
        {
            slotMetadataList = new List<SlotMetadata>();
            CreateDefaultSlotMetadata();
        }

        // 메타데이터 리스트 크기 확인 및 조정
        EnsureMetadataListSize();
    }

    // 기본 슬롯 메타데이터 생성
    private void CreateDefaultSlotMetadata()
    {
        slotMetadataList = new List<SlotMetadata>();
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            slotMetadataList.Add(new SlotMetadata());
            Debug.Log(slotMetadataList.Count);
        }

        // 로비 씬에서는 파일을 생성하지 않음
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName != "Lobby")
        {
            SaveSlotMetadata();
        }
    }

    // 메타데이터 리스트 크기 확인 및 조정
    private void EnsureMetadataListSize()
    {
        // 리스트 크기가 MAX_SLOTS보다 작으면 항목 추가
        while (slotMetadataList.Count < MAX_SLOTS)
        {
            slotMetadataList.Add(new SlotMetadata());
            Debug.Log(slotMetadataList.Count);
        }

        // 리스트 크기가 MAX_SLOTS보다 크면 초과 항목 제거
        if (slotMetadataList.Count > MAX_SLOTS)
        {
            slotMetadataList.RemoveRange(MAX_SLOTS, slotMetadataList.Count - MAX_SLOTS);
        }
    }

    // 슬롯 메타데이터 저장
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

            // 래퍼 클래스를 사용하여 List 직렬화
            SlotMetadataWrapper wrapper = new SlotMetadataWrapper { slots = slotMetadataList };
            string json = JsonUtility.ToJson(wrapper, true);
            Debug.Log($"메타데이터 JSON: {json}"); // 직렬화된 JSON 확인

            File.WriteAllText(metadataPath, json);
            Debug.Log($"메타데이터 파일 저장 완료: {metadataPath}");
            // 파일이 생성되었는지 확인
            if (File.Exists(metadataPath))
            {
                Debug.Log($"메타데이터 파일 존재 확인: 길이={new FileInfo(metadataPath).Length}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"슬롯 메타데이터 저장 실패: {e.Message}");
        }
    }

    // 슬롯 메타데이터 업데이트
    private void UpdateCurrentSlotMetadata()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"현재 씬: {currentSceneName}");

        if (currentSceneName == "Lobby")
        {
            Debug.LogWarning("로비 씬에서는 메타데이터 업데이트 건너뜀");
            return;
        }
        if (playerData != null && currentSlot < slotMetadataList.Count)
        {
            slotMetadataList[currentSlot].playerName = playerData.userID;
            // 실제 챕터 진행 상황 계산
            int maxChapterProgress = 0;
            foreach (var chapter in chapterData.chapters)
            {
                if (chapter.isUnlocked)
                {
                    int chapterNum = ExtractChapterNumber(chapter.chapterId);
                    maxChapterProgress = Mathf.Max(maxChapterProgress, chapterNum);
                }
            }

            // 계산된 최대 챕터로 메타데이터 업데이트
            slotMetadataList[currentSlot].chapterProgress = maxChapterProgress;
            slotMetadataList[currentSlot].SetLastSaveTime(DateTime.Now);
            // 총 플레이 타임은 별도로 추적 필요
            Debug.Log($"메타데이터 업데이트 시도: 슬롯={currentSlot}, 챕터={slotMetadataList[currentSlot].chapterProgress}");
            SaveSlotMetadata();
            Debug.Log("메타데이터 저장 메서드 호출 완료");
        }
    }

    // 활성 슬롯 변경
    public void SetActiveSlot(int slotIndex, bool loadData = true)
    {
        if (slotIndex >= 0 && slotIndex < MAX_SLOTS)
        {
            // 기존 데이터가 있다면 저장
            if (playerData != null && playerData.inventory != null && playerData.inventory.Count > 0)
            {
                SaveAllData();
            }

            // 슬롯 변경
            currentSlot = slotIndex;
            ((JsonSaveSystem)saveSystem).SetCurrentSlot(currentSlot);

            // loadData가 true인 경우에만 실제 데이터 로드
            if (loadData)
            {
                LoadSlotData();
            }
            else
            {
                // 메모리에 빈 객체만 생성
                playerData = new PlayerSaveData();
                chapterData = new ChapterProgressData();
            }
        }
        else
        {
            Debug.LogError($"유효하지 않은 슬롯 인덱스: {slotIndex}");
        }
    }

    // 현재 슬롯 인덱스 반환
    public int GetCurrentSlot()
    {
        return currentSlot;
    }

    // 모든 슬롯 메타데이터 반환
    public List<SlotMetadataInfo> GetAllSlotMetadata()
    {
        List<SlotMetadataInfo> result = new List<SlotMetadataInfo>();
        int originalSlot = currentSlot;
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            // 임시로 현재 슬롯 설정 (파일 생성하지 않도록 주의)

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

        // 원래 슬롯으로 되돌림
        currentSlot = originalSlot;
        ((JsonSaveSystem)saveSystem).SetCurrentSlot(currentSlot);

        return result;
    }

    // 현재 슬롯 데이터 로드
    private void LoadSlotData()
    {
        try
        {
            LoadPlayerData();
            LoadChapterData();
        }
        catch (Exception e)
        {
            Debug.LogError($"슬롯 데이터 로드 실패: {e.Message}");
            // 실패 시 기본 데이터로 초기화
            playerData = new PlayerSaveData();
            chapterData = new ChapterProgressData();
        }
    }

    // 설정 데이터 로드 (모든 슬롯이 공유)
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
                // 설정 파일이 없으면 기본값 생성
                settingsData = new GameSettingsData();

                // 로비 씬에서는 파일을 생성하지 않음
                string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (currentSceneName != "Lobby")
                {
                    SaveSettingsData();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"설정 로드 실패: {e.Message}");
            settingsData = new GameSettingsData();
        }
    }

    // 플레이어 데이터 로드
    private void LoadPlayerData()
    {
        string playerDataPath = GetSaveFilePath(PLAYER_DATA_FILE);

        // 파일이 없고, 슬롯이 이미 존재하면 기본 데이터 복사
        if (!File.Exists(playerDataPath) && ((JsonSaveSystem)saveSystem).SlotExists())
        {
            CopyDefaultDataFromStreamingAssets(PLAYER_DATA_FILE);
        }
        // 아니면 신규 데이터 생성
        else if (!File.Exists(playerDataPath))
        {
            CreateNewPlayerData();
            return;
        }

        // 파일에서 로드
        playerData = saveSystem.LoadData<PlayerSaveData>(PLAYER_DATA_FILE);

        // playerData가 null일 경우 처리
        if (playerData == null)
        {
            Debug.LogError("플레이어 데이터 로드 실패: 데이터가 null입니다.");
            CreateNewPlayerData();
            return;
        }

        // inventory가 null인지 확인
        if (playerData.inventory == null)
        {
            playerData.inventory = new List<InventoryItemData>();
        }

        // 첫 실행 시 기본 아이템 생성
        InitializeDefaultItems();
    }

    // 챕터 데이터 로드
    private void LoadChapterData()
    {
        string chapterDataPath = GetSaveFilePath(CHAPTER_PROGRESS_FILE);

        // 파일이 없고, 슬롯이 이미 존재하면 기본 데이터 복사
        if (!File.Exists(chapterDataPath) && ((JsonSaveSystem)saveSystem).SlotExists())
        {
            CopyDefaultDataFromStreamingAssets(CHAPTER_PROGRESS_FILE);
        }
        // 아니면 신규 데이터 생성
        else if (!File.Exists(chapterDataPath))
        {
            CreateNewChapterData();
            return;
        }

        // 파일에서 로드
        chapterData = saveSystem.LoadData<ChapterProgressData>(CHAPTER_PROGRESS_FILE);

        // chapterData가 null인 경우 처리
        if (chapterData == null)
        {
            Debug.LogError("챕터 데이터 로드 실패: 데이터가 null입니다.");
            CreateNewChapterData();
        }
    }

    // 스트리밍 에셋에서 기본 데이터 복사 메서드
    private void CopyDefaultDataFromStreamingAssets(string fileName)
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "DefaultSave", fileName + ".json");
        string destPath = GetSaveFilePath(fileName);

        // 스트리밍 에셋에 파일이 있는지 확인
        if (File.Exists(sourcePath))
        {
            // 저장 디렉토리가 없으면 생성
            string destDir = Path.GetDirectoryName(destPath);
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // 파일 복사
            File.Copy(sourcePath, destPath);
            Debug.Log($"기본 데이터 파일을 복사했습니다: {fileName}");
        }
        else
        {
            // 스트리밍 에셋에 파일이 없으면 기본 객체 생성
            if (fileName == PLAYER_DATA_FILE)
                CreateNewPlayerData();
            else if (fileName == CHAPTER_PROGRESS_FILE)
                CreateNewChapterData();
            else if (fileName == SETTINGS_FILE)
                CreateNewSettingsData();
        }
    }

    // 저장 파일 경로 가져오기
    private string GetSaveFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, "SaveFiles", $"Slot{currentSlot}", $"{fileName}.json");
    }

    // 첫 실행 시 기본 아이템 추가
    private void InitializeDefaultItems()
    {
        // playerData가 null인지 확인
        if (playerData == null)
        {
            Debug.LogError("playerData가 초기화되지 않았습니다.");
            return;
        }

        // inventory가 null인지 확인
        if (playerData.inventory == null)
        {
            playerData.inventory = new List<InventoryItemData>();
        }

        // 처음 실행 시 기본 아이템 추가
        if (playerData.inventory.Count == 0)
        {
            SavePlayerData();
        }
    }

    // 새 플레이어 데이터 생성
    private void CreateNewPlayerData()
    {
        playerData = new PlayerSaveData();

        // 베이스 스탯을 StatConstants 값과 일치시킴 (참조용)
        playerData.characterStats.baseHp = StatConstants.BASE_HP;
        playerData.characterStats.baseGage = StatConstants.BASE_GAGE;
        playerData.characterStats.baseAttackPower = StatConstants.BASE_ATTACK_POWER;
        playerData.characterStats.baseAttackSpeed = StatConstants.BASE_ATTACK_SPEED;
        playerData.characterStats.baseCriticalCance = StatConstants.BASE_CRITICAL_CHANCE;
        playerData.characterStats.baseSpeed = StatConstants.BASE_SPEED;
        playerData.characterStats.damageReceiveRate = StatConstants.BASE_DAMAGE_RECEIVE_RATE;

        // 업그레이드당 증가량도 일치시킴 (참조용)
        playerData.characterStats.hpPerUpgrade = StatConstants.HP_PER_UPGRADE;
        playerData.characterStats.gagePerUpgrade = StatConstants.GAGE_PER_UPGRADE;
        playerData.characterStats.attackPowerPerUpgrade = StatConstants.ATTACK_POWER_PER_UPGRADE;
        playerData.characterStats.attackSpeedPerUpgrade = StatConstants.ATTACK_SPEED_PER_UPGRADE;
        playerData.characterStats.criticalChancePerUpgrade = StatConstants.CRITICAL_CHANCE_PER_UPGRADE;
        playerData.characterStats.speedPerUpgrade = StatConstants.SPEED_PER_UPGRADE;
        playerData.characterStats.damageReducePerUpgrade = StatConstants.DAMAGE_REDUCE_PER_UPGRADE;

        // 업그레이드 카운트 초기화
        playerData.characterStats.hpUpgradeCount = 0;
        playerData.characterStats.gageUpgradeCount = 0;
        playerData.characterStats.attackPowerUpgradeCount = 0;
        playerData.characterStats.attackSpeedUpgradeCount = 0;
        playerData.characterStats.criticalChanceUpgradeCount = 0;
        playerData.characterStats.speedUpgradeCount = 0;
        playerData.characterStats.damageReduceUpgradeCount = 0;
        playerData.characterStats.UpdateTotalUpgradeCount();

        // 로비 씬에서는 파일을 생성하지 않음
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName != "Lobby")
        {
            saveSystem.SaveData(playerData, PLAYER_DATA_FILE);
            UpdateCurrentSlotMetadata();
        }

        Debug.Log("새 플레이어 데이터를 생성했습니다.");
    }

    // 새 챕터 데이터 생성
    private void CreateNewChapterData()
    {
        chapterData = new ChapterProgressData();

        // 로비 씬에서는 파일을 생성하지 않음
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName != "Lobby")
        {
            saveSystem.SaveData(chapterData, CHAPTER_PROGRESS_FILE);
        }

        Debug.Log("새 챕터 진행 데이터를 생성했습니다.");
    }

    // 새 설정 데이터 생성
    private void CreateNewSettingsData()
    {
        settingsData = new GameSettingsData();

        // 로비 씬에서는 파일을 생성하지 않음
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName != "Lobby")
        {
            saveSystem.SaveData(settingsData, SETTINGS_FILE);
        }

        Debug.Log("새 설정 데이터를 생성했습니다.");
    }

    // 게임 버전에 따른 베이스 스탯 업데이트 확인
    private void CheckForBaseStatUpdates()
    {
        // 현재 게임 버전 확인 (PlayerPrefs에 저장된 버전과 비교)
        string currentVersion = Application.version;
        string savedVersion = PlayerPrefs.GetString("GameVersion", "");

        if (currentVersion != savedVersion)
        {
            Debug.Log($"게임 버전 변경 감지: {savedVersion} -> {currentVersion}");

            // 여기에 업데이트 로직 추가
            // 업데이트 시 스탯 변경이 필요한 경우:
            // 베이스 스탯은 코드에 직접 정의되어 있으므로 별도 조정 필요 없음
            // 필요하다면 업그레이드 카운트의 효과를 조정하는 로직 추가 가능

            // 버전 정보 업데이트
            PlayerPrefs.SetString("GameVersion", currentVersion);

            // 로비 씬에서는 저장하지 않음
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentSceneName != "Lobby")
            {
                SaveAllData();
            }
        }
    }
    #endregion

    #region 저장 기능
    // 플레이어 데이터 저장
    public void SavePlayerData()
    {
        // 로비 씬에서는 파일을 생성하지 않음
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName == "Lobby")
        {
            return;
        }

        saveSystem.SaveData(playerData, PLAYER_DATA_FILE);

        UpdateCurrentSlotMetadata();
    }

    // 챕터 진행 저장
    public void SaveChapterData()
    {
        // 로비 씬에서는 파일을 생성하지 않음
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
    // 설정 저장
    public void SaveSettingsData()
    {
        // 로비 씬에서는 파일을 생성하지 않음
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName == "Lobby")
        {
            return;
        }

        // 설정을 공통 경로에 저장
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
            Debug.LogError($"설정 저장 실패: {e.Message}");
        }
    }
    // 모든 데이터 저장
    public void SaveAllData()
    {
        // 로비 씬에서는 파일을 생성하지 않음
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log(currentSceneName);
        if (currentSceneName == "Lobby")
        {
            return;
        }

        SavePlayerData();
        SaveChapterData();
        SaveSettingsData();
        Debug.Log("모든 게임 데이터 저장 완료");
    }

    // 게임 종료 시 자동 저장
    private void OnApplicationQuit()
    {
        // 로비 씬에서는 파일을 생성하지 않음
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName != "Lobby")
        {
            SaveAllData();
        }
    }
    #endregion

    #region 슬롯 관리 기능
    // 현재 슬롯 삭제
    public void DeleteCurrentSlot()
    {
        ((JsonSaveSystem)saveSystem).DeleteSlot();

        // 메타데이터 초기화
        if (currentSlot < slotMetadataList.Count)
        {
            slotMetadataList[currentSlot] = new SlotMetadata();
            SaveSlotMetadata();
        }

        // 빈 데이터로 초기화
        playerData = new PlayerSaveData();
        chapterData = new ChapterProgressData();

        Debug.Log($"슬롯 {currentSlot} 삭제 완료");
    }

    // 현재 슬롯에 데이터가 있는지 확인
    public bool CurrentSlotHasData()
    {
        return ((JsonSaveSystem)saveSystem).SlotExists();
    }
    #endregion

    #region 데이터 접근자
    // 데이터 접근자
    public PlayerSaveData GetPlayerData() => playerData;
    public ChapterProgressData GetChapterData() => chapterData;
    public GameSettingsData GetSettingsData() => settingsData;
    #endregion

    #region 데이터 업데이트
    // 플레이어 스탯 업데이트
    public void UpdatePlayerStats(Stats gameStats)
    {
        if (gameStats == null) return;

        // PlayerClass 가져오기 (GameInitializer를 통해)
        // 씬에 따라 처리 방법을 다르게 함
        PlayerClass playerClass = null;
        if (GameInitializer.Instance != null)
        {
            playerClass = GameInitializer.Instance.GetPlayerClass();
        }

        if (playerClass == null) return;

        // 옵직 업그레이드 카운트만 저장
        playerData.characterStats.hpUpgradeCount = playerClass._playerClassData.characterStats.hpUpgradeCount;
        playerData.characterStats.gageUpgradeCount = playerClass._playerClassData.characterStats.gageUpgradeCount;
        playerData.characterStats.attackPowerUpgradeCount = playerClass._playerClassData.characterStats.attackPowerUpgradeCount;
        playerData.characterStats.attackSpeedUpgradeCount = playerClass._playerClassData.characterStats.attackSpeedUpgradeCount;
        playerData.characterStats.criticalChanceUpgradeCount = playerClass._playerClassData.characterStats.criticalChanceUpgradeCount;
        playerData.characterStats.speedUpgradeCount = playerClass._playerClassData.characterStats.speedUpgradeCount;
        playerData.characterStats.damageReduceUpgradeCount = playerClass._playerClassData.characterStats.damageReduceUpgradeCount;

        // 총 업그레이드 카운트 업데이트
        playerData.characterStats.UpdateTotalUpgradeCount();

        SavePlayerData();
    }

    // 인벤토리 업데이트
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

    // 장착된 파편 업데이트
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

    // 챕터 진행 업데이트
    public void UpdateChapterProgress(string chapterId, bool completed, string record = "")
    {
        chapterData.UpdateChapter(chapterId, completed, record);
        SaveChapterData();

        // 플레이어의 현재 챕터 값도 업데이트
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

    // 챕터 ID에서 숫자 추출 (예: "YasuoChapter" -> 1)
    private int ExtractChapterNumber(string chapterId)
    {
        if (chapterId == "YasuoChapter") return 1;
        if (chapterId == "YongzokChapter") return 2;
        if (chapterId == "DeathChapter") return 3;
        if (chapterId == "HeartChapter") return 4;
        return 0;
    }
    #endregion

    #region 데이터 적용
    // 게임 초기화 시 데이터 적용
    public void ApplyGameData(PlayerClass playerClass, InventorySystem inventory, FragmentManager fragmentManager)
    {
        if (playerClass == null)
        {
            Debug.LogError("PlayerClass가 null입니다. 데이터를 적용할 수 없습니다.");
            return;
        }

        // 스탯 적용
        ApplyStatsToPlayer(playerClass);

        // 인벤토리 적용
        if (inventory != null)
        {
            ApplyInventoryItems(inventory);
        }

        // 파편 적용
        if (fragmentManager != null)
        {
            ApplyEquippedFragments(fragmentManager);
        }
    }

    // PlayerClassData에 스탯 적용
    public void ApplyStatsToPlayerClassData(PlayerClassData playerClassData)
    {
        if (playerClassData == null) return;
        if (playerData == null)
        {
            Debug.LogError("playerData가 null입니다. 스탯을 적용할 수 없습니다.");
            return;
        }

        playerClassData.userID = playerData.userID;
        playerClassData.currentChapter = playerData.currentChapter;

        // 업그레이드 카운트 적용 (베이스 스탯은 변경하지 않음)
        playerClassData.characterStats.hpUpgradeCount = playerData.characterStats.hpUpgradeCount;
        playerClassData.characterStats.gageUpgradeCount = playerData.characterStats.gageUpgradeCount;
        playerClassData.characterStats.attackPowerUpgradeCount = playerData.characterStats.attackPowerUpgradeCount;
        playerClassData.characterStats.attackSpeedUpgradeCount = playerData.characterStats.attackSpeedUpgradeCount;
        playerClassData.characterStats.criticalChanceUpgradeCount = playerData.characterStats.criticalChanceUpgradeCount;
        playerClassData.characterStats.speedUpgradeCount = playerData.characterStats.speedUpgradeCount;
        playerClassData.characterStats.damageReduceUpgradeCount = playerData.characterStats.damageReduceUpgradeCount;

        // 총 업그레이드 카운트 업데이트
        playerClassData.characterStats.UpdateTotalUpgradeCount();
    }

    // PlayerClass에 스탯 적용
    private void ApplyStatsToPlayer(PlayerClass playerClass)
    {
        if (playerClass == null) return;
        if (playerData == null)
        {
            Debug.LogError("playerData가 null입니다. 스탯을 적용할 수 없습니다.");
            return;
        }

        PlayerClassData playerClassData = playerClass._playerClassData;
        if (playerClassData == null)
        {
            Debug.LogError("playerClassData가 null입니다. 스탯을 적용할 수 없습니다.");
            return;
        }

        try
        {
            // 업그레이드 카운트 적용
            playerClassData.characterStats.hpUpgradeCount = playerData.characterStats.hpUpgradeCount;
            playerClassData.characterStats.gageUpgradeCount = playerData.characterStats.gageUpgradeCount;
            playerClassData.characterStats.attackPowerUpgradeCount = playerData.characterStats.attackPowerUpgradeCount;
            playerClassData.characterStats.attackSpeedUpgradeCount = playerData.characterStats.attackSpeedUpgradeCount;
            playerClassData.characterStats.criticalChanceUpgradeCount = playerData.characterStats.criticalChanceUpgradeCount;
            playerClassData.characterStats.speedUpgradeCount = playerData.characterStats.speedUpgradeCount;
            playerClassData.characterStats.damageReduceUpgradeCount = playerData.characterStats.damageReduceUpgradeCount;

            // 총 업그레이드 카운트 업데이트
            playerClassData.characterStats.UpdateTotalUpgradeCount();

            // 기타 플레이어 데이터 적용
            playerClassData.userID = playerData.userID;
            playerClassData.currentChapter = playerData.currentChapter;

            // 퀘스트 데이터 적용
            playerClassData.completedQuests.Clear();
            foreach (int questId in playerData.completedQuests)
            {
                playerClassData.completedQuests.Add(questId);
            }

            // 인벤토리 데이터 적용
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

            // 최종 계산된 스탯 값으로 Stats 업데이트
            Stats stats = playerClass.GetStats();
            if (stats != null)
            {
                stats.UpdateFromPlayerClassData(playerClassData);
                Debug.Log($"플레이어 스탯 적용 완료: HP {stats.MaxHealth}, 공격력 {stats.AttackPower}, 속도 {stats.Speed}");
            }
            else
            {
                Debug.LogError("PlayerClass.GetStats()가 null을 반환했습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"스탯 적용 중 오류 발생: {e.Message}");
        }
    }

    // 인벤토리 아이템 적용
    private void ApplyInventoryItems(InventorySystem inventory)
    {
        if (inventory == null) return;
        if (playerData == null || playerData.inventory == null)
        {
            Debug.LogError("playerData 또는 inventory가 null입니다. 인벤토리를 적용할 수 없습니다.");
            return;
        }

        try
        {
            // 인벤토리 초기화
            inventory.ClearInventory();

            // 저장된 아이템 추가 - 컬렉션 복사본을 사용하여 순회
            List<InventoryItemData> itemsToAdd = new List<InventoryItemData>(playerData.inventory);
            foreach (var item in itemsToAdd)
            {
                inventory.AddItem(item.itemID, item.quantity);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"인벤토리 적용 중 오류 발생: {e.Message}");
        }
    }

    // 장착된 파편 적용
    private void ApplyEquippedFragments(FragmentManager fragmentManager)
    {
        if (fragmentManager == null) return;
        if (playerData == null || playerData.equippedFragments == null)
        {
            Debug.LogError("playerData 또는 equippedFragments가 null입니다. 파편을 적용할 수 없습니다.");
            return;
        }

        try
        {
            // 기존 파편 해제
            fragmentManager.UnequipAllFragments();

            // 저장된 파편 장착
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
            Debug.LogError($"파편 적용 중 오류 발생: {e.Message}");
        }
    }
    #endregion

    #region 기타 데이터 관리
    // 디바이스 잠금 상태 저장
    public void SaveDeviceUnlockStatus(Dictionary<int, bool> deviceStatus)
    {
        if (playerData == null)
        {
            Debug.LogError("playerData가 null입니다. 디바이스 상태를 저장할 수 없습니다.");
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

    // 디바이스 잠금 상태 가져오기
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
