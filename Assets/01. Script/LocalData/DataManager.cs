// ���� DataManager Ŭ���� (ȣȯ�� ����)
using System.IO;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    // ���Ž� ���� ��� �޼���
    private string GetLegacyFilePath(string fileName) =>
        Path.Combine(Application.persistentDataPath, "SaveFiles", fileName);

    // ���� ���̺� �ʱ�ȭ �޼���
    public void InitializeNewSave(string filePath)
    {
        Debug.Log("SaveManager�� ���� ����� �����Ǿ����ϴ�.");

        // ���� ��� Ȯ�� �� ���丮 ����
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // SaveManager�� ���� ����
        SaveManager.Instance.SavePlayerData();
    }

    // ���� �ε� �޼��� (ȣȯ�� ����)
    public void LoadPlayerDataFromJson(string filePath, PlayerClassData playerData)
    {
        Debug.Log("SaveManager�� �ε� ����� �����Ǿ����ϴ�.");

        // SaveManager���� ������ ��������
        SaveManager.Instance.ApplyStatsToPlayerClassData(playerData);
    }

    // ���� ���� �޼��� (ȣȯ�� ����)
    public void SavePlayerDataToJson(string filePath, PlayerClassData playerData)
    {
        Debug.Log("SaveManager�� ���� ����� �����Ǿ����ϴ�.");

        // SaveManager�� ������ ����
        UpdatePlayerDataFromClassData(playerData);
        SaveManager.Instance.SavePlayerData();
    }

    // PlayerClassData���� ���� �����ͷ� ������Ʈ
    private void UpdatePlayerDataFromClassData(PlayerClassData playerClassData)
    {
        PlayerSaveData playerData = SaveManager.Instance.GetPlayerData();

        // �⺻ ���� ������Ʈ
        playerData.userID = playerClassData.userID;
        playerData.currentChapter = playerClassData.currentChapter;

        // ���׷��̵� ī��Ʈ�� ���� (���̽� ������ �������� ����)
        playerData.characterStats.hpUpgradeCount = playerClassData.characterStats.hpUpgradeCount;
        playerData.characterStats.gageUpgradeCount = playerClassData.characterStats.gageUpgradeCount;
        playerData.characterStats.attackPowerUpgradeCount = playerClassData.characterStats.attackPowerUpgradeCount;
        playerData.characterStats.attackSpeedUpgradeCount = playerClassData.characterStats.attackSpeedUpgradeCount;
        playerData.characterStats.criticalChanceUpgradeCount = playerClassData.characterStats.criticalChanceUpgradeCount;
        playerData.characterStats.speedUpgradeCount = playerClassData.characterStats.speedUpgradeCount;
        playerData.characterStats.damageReduceUpgradeCount = playerClassData.characterStats.damageReduceUpgradeCount;

        // �� ���׷��̵� ī��Ʈ ������Ʈ
        playerData.characterStats.UpdateTotalUpgradeCount();

        // �κ��丮 ������Ʈ
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