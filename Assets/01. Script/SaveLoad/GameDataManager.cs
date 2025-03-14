//public class GameDataManager : Singleton<GameDataManager>
//{
//    private ISaveSystem saveSystem;

//    // ������ ID ���
//    private const string PLAYER_DATA_ID = "playerData";
//    private const string CHAPTER_PROGRESS_ID = "chapterProgress";
//    private const string SETTINGS_DATA_ID = "settingsData";

//    // ĳ�̵� ������
//    private PlayerSaveData playerData;
//    private ChapterProgressData chapterProgressData;

//    private void Awake()
//    {
//        saveSystem = new JsonSaveSystem();
//        LoadAllData();
//    }

//    // ��� ������ �ε�
//    private void LoadAllData()
//    {
//        playerData = LoadPlayerData();
//        chapterProgressData = LoadChapterProgress();
//    } 

//    // �÷��̾� ������ �ε�
//    public PlayerSaveData LoadPlayerData()
//    {
//        if (playerData == null)
//        {
//            playerData = saveSystem.LoadData<PlayerSaveData>(PLAYER_DATA_ID);

//            // �ʱ� �����Ͱ� ���� ��� �⺻�� ����
//            if (playerData.userID == null)
//            {
//                playerData = new PlayerSaveData();
//                SavePlayerData();
//            }
//        }

//        return playerData;
//    }

//    // é�� ���� ������ �ε�
//    public ChapterProgressData LoadChapterProgress()
//    {
//        if (chapterProgressData == null)
//        {
//            chapterProgressData = saveSystem.LoadData<ChapterProgressData>(CHAPTER_PROGRESS_ID);
//        }

//        return chapterProgressData;
//    }

//    // �÷��̾� ������ ����
//    public void SavePlayerData()
//    {
//        saveSystem.SaveData(playerData, PLAYER_DATA_ID);
//    }

//    // é�� ���� ������ ����
//    public void SaveChapterProgress()
//    {
//        saveSystem.SaveData(chapterProgressData, CHAPTER_PROGRESS_ID);
//    }

//    // ��� ������ ����
//    public void SaveAllData()
//    {
//        SavePlayerData();
//        SaveChapterProgress();
//    }

//    // �÷��̾� ���� ������Ʈ
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

//    //é�� ���� ������Ʈ
//    public void UpdateChapterProgress(string chapterIndex, bool completed)
//    {
//        chapterProgressData.UpdateChapter(chapterIndex, completed);
//        SaveChapterProgress();
//    }

//    // �κ��丮 ������ �߰�
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