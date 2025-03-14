using System.IO;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    #region 필드 및 상수
    private ISaveSystem saveSystem;

    // 데이터 파일 이름 상수
    private const string PLAYER_DATA_FILE = "playerData";
    private const string CHAPTER_PROGRESS_FILE = "chapterProgress";
    private const string SETTINGS_FILE = "settings";

    // 캐싱된 데이터
    private PlayerSaveData playerData;
    private ChapterProgressData chapterData;
    private GameSettingsData settingsData;
    #endregion

    #region 초기화 및 기본 데이터 관리
    private void Awake()
    {
        saveSystem = new JsonSaveSystem();
        LoadAllData();
        CheckForBaseStatUpdates();
    }

    // 모든 데이터 로드
    private void LoadAllData()
    {
        // 저장 파일 경로 확인
        string playerDataPath = GetSaveFilePath(PLAYER_DATA_FILE);
        string chapterDataPath = GetSaveFilePath(CHAPTER_PROGRESS_FILE);
        string settingsDataPath = GetSaveFilePath(SETTINGS_FILE);

        // 저장 파일이 없으면 스트리밍 에셋에서 복사
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

        // 파일 로드
        playerData = saveSystem.LoadData<PlayerSaveData>(PLAYER_DATA_FILE);
        chapterData = saveSystem.LoadData<ChapterProgressData>(CHAPTER_PROGRESS_FILE);
        settingsData = saveSystem.LoadData<GameSettingsData>(SETTINGS_FILE);

        // 첫 실행 시 기본 데이터 생성
        InitializeDefaultItems();
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
        return Path.Combine(Application.persistentDataPath, "SaveFiles", $"{fileName}.json");
    }

    // 첫 실행 시 기본 아이템 추가
    private void InitializeDefaultItems()
    {
        // 처음 실행 시 기본 아이템 추가
        if (playerData.inventory.Count == 0)
        {
            playerData.inventory.Add(new InventoryItemData(3001, 3)); // 체력 포션
            playerData.inventory.Add(new InventoryItemData(3002, 1)); // 기본 재료
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

        saveSystem.SaveData(playerData, PLAYER_DATA_FILE);
        Debug.Log("새 플레이어 데이터를 생성했습니다.");
    }

    // 새 챕터 데이터 생성
    private void CreateNewChapterData()
    {
        chapterData = new ChapterProgressData();
        saveSystem.SaveData(chapterData, CHAPTER_PROGRESS_FILE);
        Debug.Log("새 챕터 진행 데이터를 생성했습니다.");
    }

    // 새 설정 데이터 생성
    private void CreateNewSettingsData()
    {
        settingsData = new GameSettingsData();
        saveSystem.SaveData(settingsData, SETTINGS_FILE);
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
            SaveAllData();
        }
    }
    #endregion

    #region 저장 기능
    // 플레이어 데이터 저장
    public void SavePlayerData()
    {
        saveSystem.SaveData(playerData, PLAYER_DATA_FILE);
    }

    // 챕터 진행 저장
    public void SaveChapterData()
    {
        saveSystem.SaveData(chapterData, CHAPTER_PROGRESS_FILE);
    }

    // 설정 저장
    public void SaveSettingsData()
    {
        saveSystem.SaveData(settingsData, SETTINGS_FILE);
    }

    // 모든 데이터 저장
    public void SaveAllData()
    {
        SavePlayerData();
        SaveChapterData();
        SaveSettingsData();
        Debug.Log("모든 게임 데이터 저장 완료");
    }

    // 게임 종료 시 자동 저장
    private void OnApplicationQuit()
    {
        SaveAllData();
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

        PlayerClass playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass == null) return;

        // 오직 업그레이드 카운트만 저장
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
    }
    #endregion

    #region 데이터 적용
    // 게임 초기화 시 데이터 적용
    public void ApplyGameData(PlayerClass playerClass, InventorySystem inventory, FragmentManager fragmentManager)
    {
        // 스탯 적용
        ApplyStatsToPlayer(playerClass);

        // 인벤토리 적용
        ApplyInventoryItems(inventory);

        // 파편 적용
        ApplyEquippedFragments(fragmentManager);
    }

    // PlayerClassData에 스탯 적용
    public void ApplyStatsToPlayerClassData(PlayerClassData playerClassData)
    {
        if (playerClassData == null) return;

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

        PlayerClassData playerClassData = playerClass._playerClassData;

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
        stats.UpdateFromPlayerClassData(playerClassData);

        // 디버그 로그
        Debug.Log($"플레이어 스탯 적용 완료: HP {stats.MaxHealth}, 공격력 {stats.AttackPower}, 속도 {stats.Speed}");
    }

    // 인벤토리 아이템 적용
    private void ApplyInventoryItems(InventorySystem inventory)
    {
        if (inventory == null) return;

        // 인벤토리 초기화
        inventory.ClearInventory();

        // 저장된 아이템 추가
        foreach (var item in playerData.inventory)
        {
            inventory.AddItem(item.itemID, item.quantity);
        }
    }

    // 장착된 파편 적용
    private void ApplyEquippedFragments(FragmentManager fragmentManager)
    {
        if (fragmentManager == null) return;

        // 기존 파편 해제
        fragmentManager.UnequipAllFragments();

        // 저장된 파편 장착
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