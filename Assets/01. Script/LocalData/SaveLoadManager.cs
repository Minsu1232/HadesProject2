using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    private void Awake()
    {
        // ���� ���� ���
        string saveDirectory = Path.Combine(Application.persistentDataPath, "SaveFiles");

        // �� �Ŵ����� ���� ���
        string playerDataPath = Path.Combine(saveDirectory, "playerData.json");
        string questDataPath = Path.Combine(saveDirectory, "quests.json");
        string inventoryDataPath = Path.Combine(saveDirectory, "inventory.json");
        string settingsDataPath = Path.Combine(saveDirectory, "settings.json");
        string weaponDataPath = Path.Combine(saveDirectory, "weaponData.json");
        // �� �Ŵ��� �ʱ�ȭ
        DataManager.Instance.InitializeNewSave(playerDataPath);
        QuestManager.Instance.InitializeQuests();
        InventoryManager.Instance.InitializeInventory();
        //SettingsManager.Instance.InitializeSettings();
        
    }
    // Start is called before the first frame update

}
