using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class InventoryData
{
    public List<InventoryItem> items = new List<InventoryItem>();
}

public class InventoryManager : Singleton<InventoryManager>
{
    public void InitializeInventory()
    {
        string path = Path.Combine(Application.persistentDataPath, "SaveFiles", "inventory.json");
        if (!File.Exists(path))
        {
            InventoryData newData = new InventoryData();
            // 초기 아이템 설정
            newData.items.Add(new InventoryItem { itemID = 101, quantity = 3 });
            newData.items.Add(new InventoryItem { itemID = 102, quantity = 1 });

            string json = JsonUtility.ToJson(newData, true);
            File.WriteAllText(path, json);
        }
    }
}