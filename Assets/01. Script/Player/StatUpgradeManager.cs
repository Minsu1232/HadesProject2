// StatUpgradeManager.cs - Material �������� ����� ���� ��ȭ ����
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class StatUpgradeManager : MonoBehaviour
{
    public static StatUpgradeManager Instance { get; private set; }

    [SerializeField] private PlayerClass playerClass;

    // ��ȭ ������ ���� Ÿ��
    public enum UpgradeableStatType
    {
        AttackPower,
        Health,
        Speed
    }

    // ���Ⱥ� �ʿ� ���� ��� ����
    private Dictionary<UpgradeableStatType, float> rarityBonusPerLevel = new Dictionary<UpgradeableStatType, float>();
    // ���Ⱥ� �ʿ� ���� ��� ����
    private Dictionary<UpgradeableStatType, List<TieredMaterial>> tieredUpgradeCosts;
    // ���Ⱥ� �ִ� ��ȭ ����
    private Dictionary<UpgradeableStatType, int> maxUpgradeLevels;

    // �̺�Ʈ: ���� ���׷��̵� �Ϸ� ��
    public delegate void StatUpgradeHandler(UpgradeableStatType statType, int newLevel);
    public event StatUpgradeHandler OnStatUpgraded;

    private string tieredMaterialsPath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // CSV ���� ��� ����
        tieredMaterialsPath = Path.Combine(Application.persistentDataPath, "UpgradeMaterials.csv");


    }

    #region CSV�ε�
    private void CopyCSVFromStreamingAssets()
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, "UpgradeMaterials.csv");

        // �׻� StreamingAssets���� �ֽ� ���Ϸ� �����
        if (File.Exists(streamingPath))
        {
            try
            {
                // ���� ������ �ִٸ� ����
                if (File.Exists(tieredMaterialsPath))
                {
                    File.Delete(tieredMaterialsPath);
                }

                // �� ���� ����
                File.Copy(streamingPath, tieredMaterialsPath);
                Debug.Log("���׷��̵� ��� CSV ���� ���� �Ϸ�");
            }
            catch (Exception e)
            {
                Debug.LogError("CSV ���� ���� �� ���� �߻�: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("StreamingAssets�� UpgradeMaterials.csv ������ �����ϴ�: " + streamingPath);
        }
    }

    private void LoadUpgradeCostsFromCSV()
    {
        tieredUpgradeCosts = new Dictionary<UpgradeableStatType, List<TieredMaterial>>();
        maxUpgradeLevels = new Dictionary<UpgradeableStatType, int>();

        if (!File.Exists(tieredMaterialsPath))
        {
            Debug.LogError("���׷��̵� ��� CSV ������ ã�� �� �����ϴ�: " + tieredMaterialsPath);
            return;
        }

        // CSV ���� �б�
        string[] lines = File.ReadAllLines(tieredMaterialsPath);

        // ��� �˻�
        if (lines.Length == 0)
        {
            Debug.LogError("CSV ������ ����ֽ��ϴ�.");
            return;
        }

        string[] headers = lines[0].Split(',');

        // ��� �����
        for (int i = 0; i < headers.Length; i++)
        {
            string header = headers[i];
            Debug.Log($"��� {i}: '{header}', ����: {header.Length}");

            // �� ���� �ڵ� ��� (������ �ʴ� ���� Ȯ��)
            string charCodes = "";
            foreach (char c in header)
            {
                charCodes += ((int)c).ToString() + " ";
            }
            Debug.Log($"��� {i} ���� �ڵ�: {charCodes}");

            // ���� ��
            if (header == "MaxLevel")
            {
                Debug.Log($"��� {i}�� ��Ȯ�� 'MaxLevel'�� ��ġ�մϴ�.");
            }
            else if (header.Trim() == "MaxLevel")
            {
                Debug.Log($"��� {i}�� ������ ������ �� 'MaxLevel'�� ��ġ�մϴ�.");
            }
        }

        // �ʼ� �÷� �ε��� ã��
        int statTypeIndex = FindColumnIndex(headers, "StatType");
        int tierIndex = FindColumnIndex(headers, "Tier");
        int itemIdIndex = FindColumnIndex(headers, "ItemID");
        int quantityIndex = FindColumnIndex(headers, "BaseQuantity");
        int maxLevelIndex = FindColumnIndex(headers, "MaxLevel");
        Debug.Log(maxLevelIndex + "!@#!@#!@#!@#!@#");
        if (statTypeIndex == -1 || tierIndex == -1 || itemIdIndex == -1 || quantityIndex == -1)
        {
            Debug.LogError("CSV ���Ͽ� �ʼ� �÷��� �����ϴ�.");
            return;
        }

        // �� ���� ó��
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');

            if (values.Length <= System.Math.Max(statTypeIndex, System.Math.Max(tierIndex, System.Math.Max(itemIdIndex, quantityIndex))))
            {
                Debug.LogWarning($"CSV �� {i + 1}�� �����Ͱ� �����մϴ�. �ǳʶݴϴ�.");
                continue;
            }

            try
            {
                // ���� Ÿ�� �Ľ�
                if (!Enum.TryParse(values[statTypeIndex], out UpgradeableStatType statType))
                {
                    Debug.LogWarning($"�� �� ���� ���� Ÿ��: {values[statTypeIndex]}");
                    continue;
                }

                // �ִ� ���� ���� (�� ����)
                if (maxLevelIndex != -1 && !maxUpgradeLevels.ContainsKey(statType))
                {
                    if (int.TryParse(values[maxLevelIndex], out int maxLevel))
                    {
                        maxUpgradeLevels[statType] = maxLevel;
                        Debug.Log($"{statType}�� �ƽ����� = {maxLevel}");
                    }
                    else
                    {
                        Debug.Log("������@@");
                    }
                }

                // Ƽ�� �Ľ�
                if (!int.TryParse(values[tierIndex], out int tier))
                {
                    Debug.LogWarning($"�߸��� Ƽ�� ��: {values[tierIndex]}");
                    continue;
                }

                // ������ ID �Ľ�
                if (!int.TryParse(values[itemIdIndex], out int itemId))
                {
                    Debug.LogWarning($"�߸��� ������ ID: {values[itemIdIndex]}");
                    continue;
                }

                // ���� �Ľ�
                if (!int.TryParse(values[quantityIndex], out int quantity))
                {
                    Debug.LogWarning($"�߸��� ����: {values[quantityIndex]}");
                    continue;
                }

                // Ƽ��� ��� ��ųʸ� �ʱ�ȭ (�ʿ��)
                if (!tieredUpgradeCosts.ContainsKey(statType))
                {
                    tieredUpgradeCosts[statType] = new List<TieredMaterial>();
                }

                // ���� Ƽ�� ã��
                TieredMaterial currentTier = tieredUpgradeCosts[statType].Find(t => t.Tier == tier);

                if (currentTier == null)
                {
                    // �� Ƽ�� ����
                    currentTier = new TieredMaterial { Tier = tier, Materials = new List<MaterialRequirement>() };
                    tieredUpgradeCosts[statType].Add(currentTier);
                }

                // ��� �߰�
                currentTier.Materials.Add(new MaterialRequirement { ItemID = itemId, BaseQuantity = quantity });
            }
            catch (Exception e)
            {
                Debug.LogError($"CSV �� {i + 1} ó�� �� ���� �߻�: {e.Message}");
            }
        }

        Debug.Log($"���׷��̵� ��� �ε� �Ϸ�: {tieredUpgradeCosts.Count}�� ���� Ÿ��");
    }

    // CSV �÷� �ε��� ã�� ���� �Լ�
    private int FindColumnIndex(string[] headers, string columnName)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i].Trim().Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("��@!#!@#!@#");
                return i;
            }
        }
        Debug.Log("����@!#!@#!@#");
        return -1; // ã�� ����
    }
    #endregion
    private void Start()
    {
        // �÷��̾� ������ ������ ã��
   
            playerClass = GameInitializer.Instance.GetPlayerClass();
        // CSV ���� ���� (������)
        CopyCSVFromStreamingAssets();

        // ��� �� �ִ� ���� ���� �ε�
        LoadUpgradeCostsFromCSV();
        Debug.Log(playerClass._playerClassData.name + "@#!#!@#!@#!@#!@#");
        // �ε�� ������ Ȯ��
        Debug.Log("�ε�� ���� Ÿ�� ��: " + tieredUpgradeCosts.Count);

        foreach (var statType in tieredUpgradeCosts.Keys)
        {
            Debug.Log($"���� Ÿ��: {statType}");
            Debug.Log($"  Ƽ�� ��: {tieredUpgradeCosts[statType].Count}");

            foreach (var tier in tieredUpgradeCosts[statType])
            {
                Debug.Log($"  Ƽ�� {tier.Tier}: ��� �� {tier.Materials.Count}");

                foreach (var material in tier.Materials)
                {
                    Debug.Log($"    ������ ID: {material.ItemID}, �⺻ ����: {material.BaseQuantity}");
                }
            }
        }

        // maxUpgradeLevels Ȯ��
        Debug.Log("�ִ� ���� ���� ��: " + maxUpgradeLevels.Count);

        foreach (var statType in maxUpgradeLevels.Keys)
        {
            Debug.Log($"���� Ÿ��: {statType}, �ִ� ����: {maxUpgradeLevels[statType]}");
        }

        // �׽�Ʈ: Ư�� ���� Ÿ���� ��� Ȯ��
        var testStatType = UpgradeableStatType.AttackPower;
        Debug.Log($"�׽�Ʈ - {testStatType}�� ���׷��̵� ���:");

        var costs = GetUpgradeCost(testStatType);
        foreach (var cost in costs)
        {
            Debug.Log($"  ������ ID: {cost.ItemID}, �ʿ� ����: {cost.RequiredQuantity}");
        }

        InitializeRarityBonuses();

    }
    // �ʱ�ȭ �޼��忡 �߰�
    private void InitializeRarityBonuses()
    {
        // �� ���� Ÿ�Ժ� ������ ��͵� ���ʽ� ����
        rarityBonusPerLevel[UpgradeableStatType.AttackPower] = 0.5f; // ���ݷ� ������ 0.5% ���
        rarityBonusPerLevel[UpgradeableStatType.Health] = 0.3f;      // ü�� ������ 0.3% ���
        rarityBonusPerLevel[UpgradeableStatType.Speed] = 0.7f;       // �ӵ� ������ 0.7% ���
    }
    // Ư�� ���� Ÿ���� ��͵� ���ʽ� ��ȯ

    // ��ü ��͵� ���ʽ� ��� (��� ������ ��)
    public float GetTotalRarityBonus()
    {
        float totalBonus = 0f;

        totalBonus += GetCurrentLevel(UpgradeableStatType.AttackPower) * rarityBonusPerLevel[UpgradeableStatType.AttackPower];
        totalBonus += GetCurrentLevel(UpgradeableStatType.Health) * rarityBonusPerLevel[UpgradeableStatType.Health];
        totalBonus += GetCurrentLevel(UpgradeableStatType.Speed) * rarityBonusPerLevel[UpgradeableStatType.Speed];

        return totalBonus;
    }
    // ���� ��ȭ ��� �ʱ�ȭ
    // �ʱ�ȭ �޼��� ����
    //private void InitializeUpgradeCosts()
    //{
    //    statUpgradeCosts = new Dictionary<UpgradeableStatType, List<MaterialRequirement>>();
    //    tieredUpgradeCosts = new Dictionary<UpgradeableStatType, List<TieredMaterial>>();
    //    maxUpgradeLevels = new Dictionary<UpgradeableStatType, int>();

    //    // ���ݷ� Ƽ� ��� ����
    //    var attackTiers = new List<TieredMaterial>
    //{
    //    new TieredMaterial {
    //        Tier = 1,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3001, BaseQuantity = 5 }, // ö ���� 5��
    //            new MaterialRequirement { ItemID = 3004, BaseQuantity = 2 }  // �Ҳ��� ���� 2��
    //        }
    //    },
    //    new TieredMaterial {
    //        Tier = 2,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3004, BaseQuantity = 5 }, // �Ҳ��� ���� 5��
    //            new MaterialRequirement { ItemID = 3007, BaseQuantity = 2 }  // ��ȭ�� �Ҳ��� ���� 2��
    //        }
    //    },
    //    new TieredMaterial {
    //        Tier = 3,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3007, BaseQuantity = 5 }, // ��ȭ�� �Ҳ��� ���� 5��
    //            new MaterialRequirement { ItemID = 3010, BaseQuantity = 1 }  // Ƽ��3 ������ 1��
    //        }
    //    }
    //};
    //    tieredUpgradeCosts[UpgradeableStatType.AttackPower] = attackTiers;

    //    // ü�� Ƽ� ��� ����
    //    var healthTiers = new List<TieredMaterial>
    //{
    //    new TieredMaterial {
    //        Tier = 1,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3002, BaseQuantity = 5 }, // ������ ���� 5��
    //            new MaterialRequirement { ItemID = 3005, BaseQuantity = 2 }  // ��ȭ�� ���� 2��
    //        }
    //    },
    //    new TieredMaterial {
    //        Tier = 2,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3005, BaseQuantity = 5 }, // ��ȭ�� ���� 5��
    //            new MaterialRequirement { ItemID = 3008, BaseQuantity = 2 }  // ��ȥ�� ����ü 2��
    //        }
    //    },
    //    new TieredMaterial {
    //        Tier = 3,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3008, BaseQuantity = 5 }, // ��ȥ�� ����ü 5��
    //            new MaterialRequirement { ItemID = 3011, BaseQuantity = 1 }  // Ƽ��3 ������ 1��
    //        }
    //    }
    //};
    //    tieredUpgradeCosts[UpgradeableStatType.Health] = healthTiers;

    //    // �̵��ӵ� Ƽ� ��� ����
    //    var speedTiers = new List<TieredMaterial>
    //{
    //    new TieredMaterial {
    //        Tier = 1,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3003, BaseQuantity = 4 }, // �ٶ��� ���� 4��
    //            new MaterialRequirement { ItemID = 3006, BaseQuantity = 1 }  // �ӵ��� ���� 1��
    //        }
    //    },
    //    new TieredMaterial {
    //        Tier = 2,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3006, BaseQuantity = 4 }, // �ӵ��� ���� 4��
    //            new MaterialRequirement { ItemID = 3009, BaseQuantity = 1 }  // �ð��� ����ü 1��
    //        }
    //    },
    //    new TieredMaterial {
    //        Tier = 3,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3009, BaseQuantity = 4 }, // �ð��� ����ü 4��
    //            new MaterialRequirement { ItemID = 3012, BaseQuantity = 1 }  // Ƽ��3 ������ 1��
    //        }
    //    }
    //};
    //    tieredUpgradeCosts[UpgradeableStatType.Speed] = speedTiers;

    //    // �ִ� ���� ����
    //    maxUpgradeLevels[UpgradeableStatType.AttackPower] = 50;
    //    maxUpgradeLevels[UpgradeableStatType.Health] = 50;
    //    maxUpgradeLevels[UpgradeableStatType.Speed] = 30;
    //}

    // ���׷��̵� ��� ��� �޼��� ����
    public List<MaterialCost> GetUpgradeCost(UpgradeableStatType statType)
    {
        if (!tieredUpgradeCosts.ContainsKey(statType))
            return new List<MaterialCost>();

        int currentLevel = GetCurrentLevel(statType);

        // ���� Ƽ�� ��� (1���� ����)
        int currentTier = (currentLevel / 10) + 1;

        // ���� Ƽ� �´� ��� ã��
        List<MaterialRequirement> currentTierMaterials = null;

        foreach (var tier in tieredUpgradeCosts[statType])
        {
            if (tier.Tier == currentTier)
            {
                currentTierMaterials = tier.Materials;
                break;
            }
        }

        // ������ Ƽ� ã�� ���ߴٸ� ���� ���� Ƽ�� ���
        if (currentTierMaterials == null)
        {
            var highestTier = tieredUpgradeCosts[statType].OrderByDescending(t => t.Tier).FirstOrDefault();
            if (highestTier != null)
            {
                currentTierMaterials = highestTier.Materials;
            }
            else
            {
                return new List<MaterialCost>();
            }
        }

        List<MaterialCost> costs = new List<MaterialCost>();

        // Ƽ�� �������� ������ ���� ���� ���� (0-9 ����)
        float levelMultiplier = 1 + (currentLevel % 10) * 0.1f; //10�۾� ����

        foreach (var requirement in currentTierMaterials)
        {
            int quantity = Mathf.CeilToInt(requirement.BaseQuantity * levelMultiplier);

            costs.Add(new MaterialCost
            {
                ItemID = requirement.ItemID,
                RequiredQuantity = quantity
            });
        }

        return costs;
    }

    // ���� ���� ���� ��������
    public int GetCurrentLevel(UpgradeableStatType statType)
    {
        if (playerClass == null || playerClass._playerClassData == null)
            return 0;

        switch (statType)
        {
            case UpgradeableStatType.AttackPower:
                return playerClass._playerClassData.characterStats.attackPowerUpgradeCount;

            case UpgradeableStatType.Health:
                return playerClass._playerClassData.characterStats.hpUpgradeCount;

            case UpgradeableStatType.Speed:
                return playerClass._playerClassData.characterStats.speedUpgradeCount;

            default:
                return 0;
        }
    }

    // ���Ⱥ� �ִ� ���� ��������
    public int GetMaxLevel(UpgradeableStatType statType)
    {
        return maxUpgradeLevels.ContainsKey(statType) ? maxUpgradeLevels[statType] : 0;
    }

    

    // ���׷��̵� ���� ���� Ȯ��
    public bool CanUpgradeStat(UpgradeableStatType statType)
    {
        // �ִ� ���� üũ
        int currentLevel = GetCurrentLevel(statType);
        int maxLevel = GetMaxLevel(statType);

        if (currentLevel >= maxLevel)
            return false;

        // ��� ������� üũ
        List<MaterialCost> costs = GetUpgradeCost(statType);

        foreach (var cost in costs)
        {
            if (!InventorySystem.Instance.HasItem(cost.ItemID, cost.RequiredQuantity))
                return false;
        }

        return true;
    }

    // ���� ���׷��̵� ����
    public bool UpgradeStat(UpgradeableStatType statType)
    {
        if (!CanUpgradeStat(statType))
            return false;

        // ��� �Һ�
        List<MaterialCost> costs = GetUpgradeCost(statType);

        foreach (var cost in costs)
        {
            InventorySystem.Instance.RemoveItem(cost.ItemID, cost.RequiredQuantity);
        }

        // ���� ���׷��̵� ����
        ApplyStatUpgrade(statType);

        // �̺�Ʈ �߻�
        OnStatUpgraded?.Invoke(statType, GetCurrentLevel(statType));

        // ����
        SaveManager.Instance.UpdatePlayerStats(playerClass.GetStats());

        return true;
    }

    // ���� ���׷��̵� ����
    private void ApplyStatUpgrade(UpgradeableStatType statType)
    {
        switch (statType)
        {
            case UpgradeableStatType.AttackPower:
                playerClass.UpgradeAttackPower();
                break;

            case UpgradeableStatType.Health:
                playerClass.UpgradeHP();
                break;

            case UpgradeableStatType.Speed:
                playerClass.UpgradeSpeed();
                break;
        }
    }

    // ���� ���׷��̵� ȿ�� ���� ��������
    public string GetStatUpgradeDescription(UpgradeableStatType statType)
    {
        int currentLevel = GetCurrentLevel(statType);
        int nextLevel = currentLevel + 1;

        switch (statType)
        {
            case UpgradeableStatType.AttackPower:
                int currentAttack = currentLevel * StatConstants.ATTACK_POWER_PER_UPGRADE;
                int nextAttack = nextLevel * StatConstants.ATTACK_POWER_PER_UPGRADE;
                return $"����: ���ݷ� +{currentAttack}\n����: ���ݷ� +{nextAttack} (+{StatConstants.ATTACK_POWER_PER_UPGRADE})";

            case UpgradeableStatType.Health:
                int currentHealth = currentLevel * StatConstants.HP_PER_UPGRADE;
                int nextHealth = nextLevel * StatConstants.HP_PER_UPGRADE;
                return $"����: ü�� +{currentHealth}\n����: ü�� +{nextHealth} (+{StatConstants.HP_PER_UPGRADE})";

            case UpgradeableStatType.Speed:
                float currentSpeed = currentLevel * StatConstants.SPEED_PER_UPGRADE * 100;
                float nextSpeed = nextLevel * StatConstants.SPEED_PER_UPGRADE * 100;
                return $"����: �̵��ӵ� +{currentSpeed:F1}%\n����: �̵��ӵ� +{nextSpeed:F1}% (+{StatConstants.SPEED_PER_UPGRADE * 100:F1}%)";

            default:
                return "���� ����";
        }
    }
}

// ��� �䱸���� Ŭ����
[System.Serializable]
public class MaterialRequirement
{
    public int ItemID;
    public int BaseQuantity;  // �⺻ �ʿ� ����
}

// ���� ������ ���� ��� ���
[System.Serializable]
public class MaterialCost
{
    public int ItemID;
    public int RequiredQuantity;  // ���� �ʿ� ����
}
// Ƽ� ��� ���Ǹ� ���� Ŭ����
[System.Serializable]
public class TieredMaterial
{
    public int Tier; // 1: ���� 1-9, 2: ���� 10-19, 3: ���� 20-29, ...
    public List<MaterialRequirement> Materials;
}