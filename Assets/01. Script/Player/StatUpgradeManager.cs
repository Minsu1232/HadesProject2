// StatUpgradeManager.cs - Material 아이템을 사용한 스탯 강화 관리
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class StatUpgradeManager : MonoBehaviour
{
    public static StatUpgradeManager Instance { get; private set; }

    [SerializeField] private PlayerClass playerClass;

    // 강화 가능한 스탯 타입
    public enum UpgradeableStatType
    {
        AttackPower,
        Health,
        Speed
    }

    // 스탯별 필요 재료와 비용 매핑
    private Dictionary<UpgradeableStatType, float> rarityBonusPerLevel = new Dictionary<UpgradeableStatType, float>();
    // 스탯별 필요 재료와 비용 매핑
    private Dictionary<UpgradeableStatType, List<TieredMaterial>> tieredUpgradeCosts;
    // 스탯별 최대 강화 레벨
    private Dictionary<UpgradeableStatType, int> maxUpgradeLevels;

    // 이벤트: 스탯 업그레이드 완료 시
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

        // CSV 파일 경로 설정
        tieredMaterialsPath = Path.Combine(Application.persistentDataPath, "UpgradeMaterials.csv");


    }

    #region CSV로드
    private void CopyCSVFromStreamingAssets()
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, "UpgradeMaterials.csv");

        // 항상 StreamingAssets에서 최신 파일로 덮어쓰기
        if (File.Exists(streamingPath))
        {
            try
            {
                // 기존 파일이 있다면 삭제
                if (File.Exists(tieredMaterialsPath))
                {
                    File.Delete(tieredMaterialsPath);
                }

                // 새 파일 복사
                File.Copy(streamingPath, tieredMaterialsPath);
                Debug.Log("업그레이드 재료 CSV 파일 복사 완료");
            }
            catch (Exception e)
            {
                Debug.LogError("CSV 파일 복사 중 오류 발생: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("StreamingAssets에 UpgradeMaterials.csv 파일이 없습니다: " + streamingPath);
        }
    }

    private void LoadUpgradeCostsFromCSV()
    {
        tieredUpgradeCosts = new Dictionary<UpgradeableStatType, List<TieredMaterial>>();
        maxUpgradeLevels = new Dictionary<UpgradeableStatType, int>();

        if (!File.Exists(tieredMaterialsPath))
        {
            Debug.LogError("업그레이드 재료 CSV 파일을 찾을 수 없습니다: " + tieredMaterialsPath);
            return;
        }

        // CSV 파일 읽기
        string[] lines = File.ReadAllLines(tieredMaterialsPath);

        // 헤더 검사
        if (lines.Length == 0)
        {
            Debug.LogError("CSV 파일이 비어있습니다.");
            return;
        }

        string[] headers = lines[0].Split(',');

        // 헤더 디버깅
        for (int i = 0; i < headers.Length; i++)
        {
            string header = headers[i];
            Debug.Log($"헤더 {i}: '{header}', 길이: {header.Length}");

            // 각 문자 코드 출력 (보이지 않는 문자 확인)
            string charCodes = "";
            foreach (char c in header)
            {
                charCodes += ((int)c).ToString() + " ";
            }
            Debug.Log($"헤더 {i} 문자 코드: {charCodes}");

            // 직접 비교
            if (header == "MaxLevel")
            {
                Debug.Log($"헤더 {i}는 정확히 'MaxLevel'과 일치합니다.");
            }
            else if (header.Trim() == "MaxLevel")
            {
                Debug.Log($"헤더 {i}는 공백을 제거한 후 'MaxLevel'과 일치합니다.");
            }
        }

        // 필수 컬럼 인덱스 찾기
        int statTypeIndex = FindColumnIndex(headers, "StatType");
        int tierIndex = FindColumnIndex(headers, "Tier");
        int itemIdIndex = FindColumnIndex(headers, "ItemID");
        int quantityIndex = FindColumnIndex(headers, "BaseQuantity");
        int maxLevelIndex = FindColumnIndex(headers, "MaxLevel");
        Debug.Log(maxLevelIndex + "!@#!@#!@#!@#!@#");
        if (statTypeIndex == -1 || tierIndex == -1 || itemIdIndex == -1 || quantityIndex == -1)
        {
            Debug.LogError("CSV 파일에 필수 컬럼이 없습니다.");
            return;
        }

        // 각 라인 처리
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');

            if (values.Length <= System.Math.Max(statTypeIndex, System.Math.Max(tierIndex, System.Math.Max(itemIdIndex, quantityIndex))))
            {
                Debug.LogWarning($"CSV 줄 {i + 1}에 데이터가 부족합니다. 건너뜁니다.");
                continue;
            }

            try
            {
                // 스탯 타입 파싱
                if (!Enum.TryParse(values[statTypeIndex], out UpgradeableStatType statType))
                {
                    Debug.LogWarning($"알 수 없는 스탯 타입: {values[statTypeIndex]}");
                    continue;
                }

                // 최대 레벨 설정 (한 번만)
                if (maxLevelIndex != -1 && !maxUpgradeLevels.ContainsKey(statType))
                {
                    if (int.TryParse(values[maxLevelIndex], out int maxLevel))
                    {
                        maxUpgradeLevels[statType] = maxLevel;
                        Debug.Log($"{statType}의 맥스레벨 = {maxLevel}");
                    }
                    else
                    {
                        Debug.Log("뫃ㅅ옴@@");
                    }
                }

                // 티어 파싱
                if (!int.TryParse(values[tierIndex], out int tier))
                {
                    Debug.LogWarning($"잘못된 티어 값: {values[tierIndex]}");
                    continue;
                }

                // 아이템 ID 파싱
                if (!int.TryParse(values[itemIdIndex], out int itemId))
                {
                    Debug.LogWarning($"잘못된 아이템 ID: {values[itemIdIndex]}");
                    continue;
                }

                // 수량 파싱
                if (!int.TryParse(values[quantityIndex], out int quantity))
                {
                    Debug.LogWarning($"잘못된 수량: {values[quantityIndex]}");
                    continue;
                }

                // 티어드 재료 딕셔너리 초기화 (필요시)
                if (!tieredUpgradeCosts.ContainsKey(statType))
                {
                    tieredUpgradeCosts[statType] = new List<TieredMaterial>();
                }

                // 현재 티어 찾기
                TieredMaterial currentTier = tieredUpgradeCosts[statType].Find(t => t.Tier == tier);

                if (currentTier == null)
                {
                    // 새 티어 생성
                    currentTier = new TieredMaterial { Tier = tier, Materials = new List<MaterialRequirement>() };
                    tieredUpgradeCosts[statType].Add(currentTier);
                }

                // 재료 추가
                currentTier.Materials.Add(new MaterialRequirement { ItemID = itemId, BaseQuantity = quantity });
            }
            catch (Exception e)
            {
                Debug.LogError($"CSV 줄 {i + 1} 처리 중 오류 발생: {e.Message}");
            }
        }

        Debug.Log($"업그레이드 재료 로드 완료: {tieredUpgradeCosts.Count}개 스탯 타입");
    }

    // CSV 컬럼 인덱스 찾기 헬퍼 함수
    private int FindColumnIndex(string[] headers, string columnName)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i].Trim().Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("뙴@!#!@#!@#");
                return i;
            }
        }
        Debug.Log("못뙴@!#!@#!@#");
        return -1; // 찾지 못함
    }
    #endregion
    private void Start()
    {
        // 플레이어 참조가 없으면 찾기
   
            playerClass = GameInitializer.Instance.GetPlayerClass();
        // CSV 파일 복사 (없으면)
        CopyCSVFromStreamingAssets();

        // 재료 및 최대 레벨 정보 로드
        LoadUpgradeCostsFromCSV();
        Debug.Log(playerClass._playerClassData.name + "@#!#!@#!@#!@#!@#");
        // 로드된 데이터 확인
        Debug.Log("로드된 스탯 타입 수: " + tieredUpgradeCosts.Count);

        foreach (var statType in tieredUpgradeCosts.Keys)
        {
            Debug.Log($"스탯 타입: {statType}");
            Debug.Log($"  티어 수: {tieredUpgradeCosts[statType].Count}");

            foreach (var tier in tieredUpgradeCosts[statType])
            {
                Debug.Log($"  티어 {tier.Tier}: 재료 수 {tier.Materials.Count}");

                foreach (var material in tier.Materials)
                {
                    Debug.Log($"    아이템 ID: {material.ItemID}, 기본 수량: {material.BaseQuantity}");
                }
            }
        }

        // maxUpgradeLevels 확인
        Debug.Log("최대 레벨 설정 수: " + maxUpgradeLevels.Count);

        foreach (var statType in maxUpgradeLevels.Keys)
        {
            Debug.Log($"스탯 타입: {statType}, 최대 레벨: {maxUpgradeLevels[statType]}");
        }

        // 테스트: 특정 스탯 타입의 비용 확인
        var testStatType = UpgradeableStatType.AttackPower;
        Debug.Log($"테스트 - {testStatType}의 업그레이드 비용:");

        var costs = GetUpgradeCost(testStatType);
        foreach (var cost in costs)
        {
            Debug.Log($"  아이템 ID: {cost.ItemID}, 필요 수량: {cost.RequiredQuantity}");
        }

        InitializeRarityBonuses();

    }
    // 초기화 메서드에 추가
    private void InitializeRarityBonuses()
    {
        // 각 스탯 타입별 레벨당 희귀도 보너스 설정
        rarityBonusPerLevel[UpgradeableStatType.AttackPower] = 0.5f; // 공격력 레벨당 0.5% 향상
        rarityBonusPerLevel[UpgradeableStatType.Health] = 0.3f;      // 체력 레벨당 0.3% 향상
        rarityBonusPerLevel[UpgradeableStatType.Speed] = 0.7f;       // 속도 레벨당 0.7% 향상
    }
    // 특정 스탯 타입의 희귀도 보너스 반환

    // 전체 희귀도 보너스 계산 (모든 스탯의 합)
    public float GetTotalRarityBonus()
    {
        float totalBonus = 0f;

        totalBonus += GetCurrentLevel(UpgradeableStatType.AttackPower) * rarityBonusPerLevel[UpgradeableStatType.AttackPower];
        totalBonus += GetCurrentLevel(UpgradeableStatType.Health) * rarityBonusPerLevel[UpgradeableStatType.Health];
        totalBonus += GetCurrentLevel(UpgradeableStatType.Speed) * rarityBonusPerLevel[UpgradeableStatType.Speed];

        return totalBonus;
    }
    // 스탯 강화 비용 초기화
    // 초기화 메서드 수정
    //private void InitializeUpgradeCosts()
    //{
    //    statUpgradeCosts = new Dictionary<UpgradeableStatType, List<MaterialRequirement>>();
    //    tieredUpgradeCosts = new Dictionary<UpgradeableStatType, List<TieredMaterial>>();
    //    maxUpgradeLevels = new Dictionary<UpgradeableStatType, int>();

    //    // 공격력 티어별 재료 설정
    //    var attackTiers = new List<TieredMaterial>
    //{
    //    new TieredMaterial {
    //        Tier = 1,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3001, BaseQuantity = 5 }, // 철 광석 5개
    //            new MaterialRequirement { ItemID = 3004, BaseQuantity = 2 }  // 불꽃의 결정 2개
    //        }
    //    },
    //    new TieredMaterial {
    //        Tier = 2,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3004, BaseQuantity = 5 }, // 불꽃의 결정 5개
    //            new MaterialRequirement { ItemID = 3007, BaseQuantity = 2 }  // 강화된 불꽃의 결정 2개
    //        }
    //    },
    //    new TieredMaterial {
    //        Tier = 3,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3007, BaseQuantity = 5 }, // 강화된 불꽃의 결정 5개
    //            new MaterialRequirement { ItemID = 3010, BaseQuantity = 1 }  // 티어3 아이템 1개
    //        }
    //    }
    //};
    //    tieredUpgradeCosts[UpgradeableStatType.AttackPower] = attackTiers;

    //    // 체력 티어별 재료 설정
    //    var healthTiers = new List<TieredMaterial>
    //{
    //    new TieredMaterial {
    //        Tier = 1,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3002, BaseQuantity = 5 }, // 생명의 수액 5개
    //            new MaterialRequirement { ItemID = 3005, BaseQuantity = 2 }  // 강화된 조각 2개
    //        }
    //    },
    //    new TieredMaterial {
    //        Tier = 2,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3005, BaseQuantity = 5 }, // 강화된 조각 5개
    //            new MaterialRequirement { ItemID = 3008, BaseQuantity = 2 }  // 영혼의 결정체 2개
    //        }
    //    },
    //    new TieredMaterial {
    //        Tier = 3,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3008, BaseQuantity = 5 }, // 영혼의 결정체 5개
    //            new MaterialRequirement { ItemID = 3011, BaseQuantity = 1 }  // 티어3 아이템 1개
    //        }
    //    }
    //};
    //    tieredUpgradeCosts[UpgradeableStatType.Health] = healthTiers;

    //    // 이동속도 티어별 재료 설정
    //    var speedTiers = new List<TieredMaterial>
    //{
    //    new TieredMaterial {
    //        Tier = 1,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3003, BaseQuantity = 4 }, // 바람의 결정 4개
    //            new MaterialRequirement { ItemID = 3006, BaseQuantity = 1 }  // 속도의 파편 1개
    //        }
    //    },
    //    new TieredMaterial {
    //        Tier = 2,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3006, BaseQuantity = 4 }, // 속도의 파편 4개
    //            new MaterialRequirement { ItemID = 3009, BaseQuantity = 1 }  // 시간의 결정체 1개
    //        }
    //    },
    //    new TieredMaterial {
    //        Tier = 3,
    //        Materials = new List<MaterialRequirement> {
    //            new MaterialRequirement { ItemID = 3009, BaseQuantity = 4 }, // 시간의 결정체 4개
    //            new MaterialRequirement { ItemID = 3012, BaseQuantity = 1 }  // 티어3 아이템 1개
    //        }
    //    }
    //};
    //    tieredUpgradeCosts[UpgradeableStatType.Speed] = speedTiers;

    //    // 최대 레벨 설정
    //    maxUpgradeLevels[UpgradeableStatType.AttackPower] = 50;
    //    maxUpgradeLevels[UpgradeableStatType.Health] = 50;
    //    maxUpgradeLevels[UpgradeableStatType.Speed] = 30;
    //}

    // 업그레이드 비용 계산 메서드 수정
    public List<MaterialCost> GetUpgradeCost(UpgradeableStatType statType)
    {
        if (!tieredUpgradeCosts.ContainsKey(statType))
            return new List<MaterialCost>();

        int currentLevel = GetCurrentLevel(statType);

        // 현재 티어 계산 (1부터 시작)
        int currentTier = (currentLevel / 10) + 1;

        // 현재 티어에 맞는 재료 찾기
        List<MaterialRequirement> currentTierMaterials = null;

        foreach (var tier in tieredUpgradeCosts[statType])
        {
            if (tier.Tier == currentTier)
            {
                currentTierMaterials = tier.Materials;
                break;
            }
        }

        // 적절한 티어를 찾지 못했다면 가장 높은 티어 사용
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

        // 티어 내에서의 레벨에 따른 수량 증가 (0-9 레벨)
        float levelMultiplier = 1 + (currentLevel % 10) * 0.1f; //10퍼씩 증가

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

    // 현재 스탯 레벨 가져오기
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

    // 스탯별 최대 레벨 가져오기
    public int GetMaxLevel(UpgradeableStatType statType)
    {
        return maxUpgradeLevels.ContainsKey(statType) ? maxUpgradeLevels[statType] : 0;
    }

    

    // 업그레이드 가능 여부 확인
    public bool CanUpgradeStat(UpgradeableStatType statType)
    {
        // 최대 레벨 체크
        int currentLevel = GetCurrentLevel(statType);
        int maxLevel = GetMaxLevel(statType);

        if (currentLevel >= maxLevel)
            return false;

        // 재료 충분한지 체크
        List<MaterialCost> costs = GetUpgradeCost(statType);

        foreach (var cost in costs)
        {
            if (!InventorySystem.Instance.HasItem(cost.ItemID, cost.RequiredQuantity))
                return false;
        }

        return true;
    }

    // 스탯 업그레이드 실행
    public bool UpgradeStat(UpgradeableStatType statType)
    {
        if (!CanUpgradeStat(statType))
            return false;

        // 재료 소비
        List<MaterialCost> costs = GetUpgradeCost(statType);

        foreach (var cost in costs)
        {
            InventorySystem.Instance.RemoveItem(cost.ItemID, cost.RequiredQuantity);
        }

        // 스탯 업그레이드 적용
        ApplyStatUpgrade(statType);

        // 이벤트 발생
        OnStatUpgraded?.Invoke(statType, GetCurrentLevel(statType));

        // 저장
        SaveManager.Instance.UpdatePlayerStats(playerClass.GetStats());

        return true;
    }

    // 스탯 업그레이드 적용
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

    // 스탯 업그레이드 효과 설명 가져오기
    public string GetStatUpgradeDescription(UpgradeableStatType statType)
    {
        int currentLevel = GetCurrentLevel(statType);
        int nextLevel = currentLevel + 1;

        switch (statType)
        {
            case UpgradeableStatType.AttackPower:
                int currentAttack = currentLevel * StatConstants.ATTACK_POWER_PER_UPGRADE;
                int nextAttack = nextLevel * StatConstants.ATTACK_POWER_PER_UPGRADE;
                return $"현재: 공격력 +{currentAttack}\n다음: 공격력 +{nextAttack} (+{StatConstants.ATTACK_POWER_PER_UPGRADE})";

            case UpgradeableStatType.Health:
                int currentHealth = currentLevel * StatConstants.HP_PER_UPGRADE;
                int nextHealth = nextLevel * StatConstants.HP_PER_UPGRADE;
                return $"현재: 체력 +{currentHealth}\n다음: 체력 +{nextHealth} (+{StatConstants.HP_PER_UPGRADE})";

            case UpgradeableStatType.Speed:
                float currentSpeed = currentLevel * StatConstants.SPEED_PER_UPGRADE * 100;
                float nextSpeed = nextLevel * StatConstants.SPEED_PER_UPGRADE * 100;
                return $"현재: 이동속도 +{currentSpeed:F1}%\n다음: 이동속도 +{nextSpeed:F1}% (+{StatConstants.SPEED_PER_UPGRADE * 100:F1}%)";

            default:
                return "정보 없음";
        }
    }
}

// 재료 요구사항 클래스
[System.Serializable]
public class MaterialRequirement
{
    public int ItemID;
    public int BaseQuantity;  // 기본 필요 수량
}

// 현재 레벨의 실제 재료 비용
[System.Serializable]
public class MaterialCost
{
    public int ItemID;
    public int RequiredQuantity;  // 현재 필요 수량
}
// 티어별 재료 정의를 위한 클래스
[System.Serializable]
public class TieredMaterial
{
    public int Tier; // 1: 레벨 1-9, 2: 레벨 10-19, 3: 레벨 20-29, ...
    public List<MaterialRequirement> Materials;
}