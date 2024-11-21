using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[System.Serializable]
public class CharacterStats
{
    public float baseHp = 100;
    public float baseGage = 0;
    public float baseAttackPower = 10;
    public float baseAttackSpeed = 1f;
    public float baseCriticalCance = 0.3f;
    public float baseSpeed = 1;
    public int upgradeCount = 3;
}

[System.Serializable]
public class InventoryItem
{
    public int itemID;
    public int quantity;
}

[System.Serializable]
public class PlayerSaveData
{
    public string userID = "Player";
    public int currentChapter = 1;
    public List<string> completedQuests = new List<string>();
    public CharacterStats characterStats = new CharacterStats();
    public List<InventoryItem> inventory = new List<InventoryItem>();
}

public class DataManager : Singleton<DataManager>
{
    public void InitializeNewSave(string filePath)
    {
        // ������ ���ٸ� ����
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // ������ ���� ��쿡�� ���� ����
        if (!File.Exists(filePath))
        {
            PlayerSaveData newSaveData = new PlayerSaveData();

            // �⺻ �κ��丮 ������ �߰�
            newSaveData.inventory.Add(new InventoryItem { itemID = 101, quantity = 3 });
            newSaveData.inventory.Add(new InventoryItem { itemID = 102, quantity = 1 });

            // JSON���� ��ȯ �� ����
            string jsonData = JsonUtility.ToJson(newSaveData, true); // true�� ���� ���� ������
            File.WriteAllText(filePath, jsonData);
            Debug.Log($"�� ���̺� ������ �����Ǿ����ϴ�: {filePath}");
        }
    }

    public void LoadPlayerDataFromJson(string filePath, PlayerClassData playerData)
    {
        // ������ ������ ���� ����
        if (!File.Exists(filePath))
        {
            Debug.Log("���̺� ������ ���� ���� �����մϴ�.");
            InitializeNewSave(filePath);
        }

        string json = File.ReadAllText(filePath);
        Debug.Log("JSON ����: " + json);
        JsonUtility.FromJsonOverwrite(json, playerData);
        Debug.Log("�ε� �� ������: " + JsonUtility.ToJson(playerData));
    }

    // ������: ���� �����͸� �����ϴ� �޼���
    public void SavePlayerDataToJson(string filePath, PlayerClassData playerData)
    {
        string jsonData = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(filePath, jsonData);
        Debug.Log($"�����Ͱ� ����Ǿ����ϴ�: {filePath}");
    }
}
