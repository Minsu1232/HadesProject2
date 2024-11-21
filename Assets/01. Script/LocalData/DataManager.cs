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
        // 폴더가 없다면 생성
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // 파일이 없을 경우에만 새로 생성
        if (!File.Exists(filePath))
        {
            PlayerSaveData newSaveData = new PlayerSaveData();

            // 기본 인벤토리 아이템 추가
            newSaveData.inventory.Add(new InventoryItem { itemID = 101, quantity = 3 });
            newSaveData.inventory.Add(new InventoryItem { itemID = 102, quantity = 1 });

            // JSON으로 변환 및 저장
            string jsonData = JsonUtility.ToJson(newSaveData, true); // true는 보기 좋게 포맷팅
            File.WriteAllText(filePath, jsonData);
            Debug.Log($"새 세이브 파일이 생성되었습니다: {filePath}");
        }
    }

    public void LoadPlayerDataFromJson(string filePath, PlayerClassData playerData)
    {
        // 파일이 없으면 새로 생성
        if (!File.Exists(filePath))
        {
            Debug.Log("세이브 파일이 없어 새로 생성합니다.");
            InitializeNewSave(filePath);
        }

        string json = File.ReadAllText(filePath);
        Debug.Log("JSON 내용: " + json);
        JsonUtility.FromJsonOverwrite(json, playerData);
        Debug.Log("로드 후 데이터: " + JsonUtility.ToJson(playerData));
    }

    // 선택적: 현재 데이터를 저장하는 메서드
    public void SavePlayerDataToJson(string filePath, PlayerClassData playerData)
    {
        string jsonData = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(filePath, jsonData);
        Debug.Log($"데이터가 저장되었습니다: {filePath}");
    }
}
