using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    private void Awake()
    {
        // 공통 저장 경로
        string saveDirectory = Path.Combine(Application.persistentDataPath, "SaveFiles");

        // 각 매니저별 파일 경로
        string playerDataPath = Path.Combine(saveDirectory, "playerData.json");
        string questDataPath = Path.Combine(saveDirectory, "quests.json");
        string inventoryDataPath = Path.Combine(saveDirectory, "inventory.json");
        string settingsDataPath = Path.Combine(saveDirectory, "settings.json");
        string weaponDataPath = Path.Combine(saveDirectory, "weaponData.json");
        // 각 매니저 초기화
        DataManager.Instance.InitializeNewSave(playerDataPath);
        QuestManager.Instance.InitializeQuests();
        InventoryManager.Instance.InitializeInventory();
        //SettingsManager.Instance.InitializeSettings();
        
    }
    // Start is called before the first frame update

}
