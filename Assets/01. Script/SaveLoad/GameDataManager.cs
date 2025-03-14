//public class GameDataManager : Singleton<GameDataManager>
//{
//    private ISaveSystem saveSystem;

//    // 데이터 ID 상수
//    private const string PLAYER_DATA_ID = "playerData";
//    private const string CHAPTER_PROGRESS_ID = "chapterProgress";
//    private const string SETTINGS_DATA_ID = "settingsData";

//    // 캐싱된 데이터
//    private PlayerSaveData playerData;
//    private ChapterProgressData chapterProgressData;

//    private void Awake()
//    {
//        saveSystem = new JsonSaveSystem();
//        LoadAllData();
//    }

//    // 모든 데이터 로드
//    private void LoadAllData()
//    {
//        playerData = LoadPlayerData();
//        chapterProgressData = LoadChapterProgress();
//    } 

//    // 플레이어 데이터 로드
//    public PlayerSaveData LoadPlayerData()
//    {
//        if (playerData == null)
//        {
//            playerData = saveSystem.LoadData<PlayerSaveData>(PLAYER_DATA_ID);

//            // 초기 데이터가 없는 경우 기본값 생성
//            if (playerData.userID == null)
//            {
//                playerData = new PlayerSaveData();
//                SavePlayerData();
//            }
//        }

//        return playerData;
//    }

//    // 챕터 진행 데이터 로드
//    public ChapterProgressData LoadChapterProgress()
//    {
//        if (chapterProgressData == null)
//        {
//            chapterProgressData = saveSystem.LoadData<ChapterProgressData>(CHAPTER_PROGRESS_ID);
//        }

//        return chapterProgressData;
//    }

//    // 플레이어 데이터 저장
//    public void SavePlayerData()
//    {
//        saveSystem.SaveData(playerData, PLAYER_DATA_ID);
//    }

//    // 챕터 진행 데이터 저장
//    public void SaveChapterProgress()
//    {
//        saveSystem.SaveData(chapterProgressData, CHAPTER_PROGRESS_ID);
//    }

//    // 모든 데이터 저장
//    public void SaveAllData()
//    {
//        SavePlayerData();
//        SaveChapterProgress();
//    }

//    // 플레이어 스탯 업데이트
//    public void UpdatePlayerStats(Stats updatedStats)
//    {
//        playerData.characterStats.baseHp = updatedStats.MaxHealth;
//        playerData.characterStats.baseGage = updatedStats.MaxMana;
//        playerData.characterStats.baseAttackPower = updatedStats.AttackPower;
//        playerData.characterStats.baseAttackSpeed = updatedStats.AttackSpeed;
//        playerData.characterStats.baseSpeed = updatedStats.Speed;
//        playerData.characterStats.baseCriticalCance = updatedStats.CriticalChance;
//        playerData.characterStats.damageReceiveRate = updatedStats.DamageReceiveRate;

//        SavePlayerData();
//    }

//    //챕터 진행 업데이트
//    public void UpdateChapterProgress(string chapterIndex, bool completed)
//    {
//        chapterProgressData.UpdateChapter(chapterIndex, completed);
//        SaveChapterProgress();
//    }

//    // 인벤토리 아이템 추가
//    //public void AddInventoryItem(int itemId, int quantity)
//    //{
//    //    var existingItem = playerData.inventory.Find(item => item.itemID == itemId);

//    //    if (existingItem != null)
//    //    {
//    //        existingItem.quantity += quantity;
//    //    }
//    //    else
//    //    {
//    //        playerData.inventory.Add(new InventoryItem { itemID = itemId, quantity = quantity });
//    //    }

//    //    SavePlayerData();
//    //}
//}