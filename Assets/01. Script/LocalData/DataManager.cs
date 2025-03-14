// 기존 DataManager 클래스 (호환성 유지)
using System.IO;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    // 레거시 저장 경로 메서드
    private string GetLegacyFilePath(string fileName) =>
        Path.Combine(Application.persistentDataPath, "SaveFiles", fileName);

    // 기존 세이브 초기화 메서드
    public void InitializeNewSave(string filePath)
    {
        Debug.Log("SaveManager로 저장 기능이 이전되었습니다.");

        // 파일 경로 확인 및 디렉토리 생성
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // SaveManager를 통해 저장
        SaveManager.Instance.SavePlayerData();
    }

    // 기존 로드 메서드 (호환성 유지)
    public void LoadPlayerDataFromJson(string filePath, PlayerClassData playerData)
    {
        Debug.Log("SaveManager로 로드 기능이 이전되었습니다.");

        // SaveManager에서 데이터 가져오기
        SaveManager.Instance.ApplyStatsToPlayerClassData(playerData);
    }

    // 기존 저장 메서드 (호환성 유지)
    public void SavePlayerDataToJson(string filePath, PlayerClassData playerData)
    {
        Debug.Log("SaveManager로 저장 기능이 이전되었습니다.");

        // SaveManager로 데이터 전달
        UpdatePlayerDataFromClassData(playerData);
        SaveManager.Instance.SavePlayerData();
    }

    // PlayerClassData에서 저장 데이터로 업데이트
    private void UpdatePlayerDataFromClassData(PlayerClassData playerClassData)
    {
        PlayerSaveData playerData = SaveManager.Instance.GetPlayerData();

        // 기본 정보 업데이트
        playerData.userID = playerClassData.userID;
        playerData.currentChapter = playerClassData.currentChapter;

        // 업그레이드 카운트만 저장 (베이스 스탯은 변경하지 않음)
        playerData.characterStats.hpUpgradeCount = playerClassData.characterStats.hpUpgradeCount;
        playerData.characterStats.gageUpgradeCount = playerClassData.characterStats.gageUpgradeCount;
        playerData.characterStats.attackPowerUpgradeCount = playerClassData.characterStats.attackPowerUpgradeCount;
        playerData.characterStats.attackSpeedUpgradeCount = playerClassData.characterStats.attackSpeedUpgradeCount;
        playerData.characterStats.criticalChanceUpgradeCount = playerClassData.characterStats.criticalChanceUpgradeCount;
        playerData.characterStats.speedUpgradeCount = playerClassData.characterStats.speedUpgradeCount;
        playerData.characterStats.damageReduceUpgradeCount = playerClassData.characterStats.damageReduceUpgradeCount;

        // 총 업그레이드 카운트 업데이트
        playerData.characterStats.UpdateTotalUpgradeCount();

        // 인벤토리 업데이트
        playerData.inventory.Clear();
        foreach (var item in playerClassData.inventory)
        {
            playerData.inventory.Add(new InventoryItemData(
                item.itemID,
                item.quantity
            ));
        }
    }
}