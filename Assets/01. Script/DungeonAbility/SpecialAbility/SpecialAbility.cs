// SpecialAbility.cs - 특수 스킬 관련 능력 클래스 (DungeonAbility 상속)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class SpecialAbility : DungeonAbility
{
    public enum SpecialAbilityType
    {
        ResourceRetention,// 스킬 사용 후 게이지 일부 보존
        LightningJudgment // 모든 적에게 번개 공격
        // 추후 다른 특수 스킬 관련 능력들 추가 가능
    }

    public SpecialAbilityType specialType;

    // 생성자로 초기화
    public void Initialize(SpecialAbilityType type, float value, string abilityName, string abilityDescription, Rarity abilityRarity)
    {
        specialType = type;
        effectValue = value;
        originalValue = value;

        id = $"special_{type.ToString().ToLower()}";
        name = abilityName;
        description = abilityDescription;
        rarity = abilityRarity;
    }

    // CSV 데이터로 초기화하는 메서드 추가
    public static SpecialAbility FromCSVData(Dictionary<string, string> csvData)
    {
        SpecialAbility ability = new SpecialAbility();

        // 필수 값 파싱
        string id = csvData["ID"];
        SpecialAbilityType type = (SpecialAbilityType)System.Enum.Parse(typeof(SpecialAbilityType), csvData["Type"]);
        string name = csvData["Name"];
        string description = csvData["Description"];
        Rarity rarity = (Rarity)int.Parse(csvData["Rarity"]);
        float baseValue = float.Parse(csvData["BaseValue"]);

        // 레벨업 배율 파싱 추가
        if (csvData.ContainsKey("LevelMultiplier") && !string.IsNullOrEmpty(csvData["LevelMultiplier"]))
        {
            ability.levelMultiplier = float.Parse(csvData["LevelMultiplier"]);
        }

        // 능력 초기화
        ability.Initialize(type, baseValue, name, description, rarity);
        ability.id = id; // ID 값 덮어쓰기 (유니크한 ID 사용)

        // 아이콘 경로가 있다면 어드레서블로 로드
        if (csvData.ContainsKey("IconPath") && !string.IsNullOrEmpty(csvData["IconPath"]))
        {
            string iconAddress = csvData["IconPath"];
            Debug.Log(iconAddress);
            // 애드레서블 비동기 로드
            Addressables.LoadAssetAsync<Sprite>(iconAddress).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    ability.icon = handle.Result;
                    Debug.Log($"아이콘 로드 성공: {iconAddress}");
                }
                else
                {
                    Debug.LogWarning($"아이콘을 로드할 수 없습니다: {iconAddress}");
                }
            };
        }
        else
        {
            Debug.LogWarning($"아이콘을 로드할 수 없습니다");
        }

        return ability;
    }

    public override void OnAcquire(PlayerClass player)
    {
        // 특수 효과 적용
        ApplyEffect(player, effectValue);
        Debug.Log($"획득한 특수 스킬 능력: {name} (Lv.{level}) - {effectValue}");
    }

    public override void OnLevelUp(PlayerClass player)
    {
        // 레벨업 플래그 설정
        isLevelingUp = true;

        // 기존 효과 제거
        OnReset(player);

        // 레벨업 및 효과 증가
        level++;
        effectValue = originalValue * (1 + (level - 1) * levelMultiplier);

        // 새 효과 적용
        OnAcquire(player);

        // 레벨업 플래그 해제
        isLevelingUp = false;

        Debug.Log($"레벨업한 특수 스킬 능력: {name} (Lv.{level}) - {effectValue}");
    }

    public override void OnReset(PlayerClass player)
    {
        // 특수 효과 제거
        RemoveEffect(player, effectValue);
    }

    // 특수 효과 적용
    protected override void ApplyEffect(PlayerClass player, float value)
    {
        switch (specialType)
        {
            case SpecialAbilityType.ResourceRetention:
                ApplyResourceRetention(player, value);
                break;
            case SpecialAbilityType.LightningJudgment:
                ApplyLightningJudgment(player, value);
                break;
                // 추후 다른 케이스 추가
        }
    }

    // 특수 효과 제거
    protected override void RemoveEffect(PlayerClass player, float value)
    {
        switch (specialType)
        {
            case SpecialAbilityType.ResourceRetention:
                RemoveResourceRetention(player, value);
                break;
            case SpecialAbilityType.LightningJudgment:
                RemoveLightningJudgment(player, value);
                break;
                // 추후 다른 케이스 추가
        }
    }

    // 게이지 보존 효과 적용
    private void ApplyResourceRetention(PlayerClass player, float retentionPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        SpecialResourceRetentionComponent retentionComp = playerObj.GetComponent<SpecialResourceRetentionComponent>();
        if (retentionComp == null)
        {
            retentionComp = playerObj.AddComponent<SpecialResourceRetentionComponent>();
        }

        retentionComp.AddRetentionRate(retentionPercent / 100f);
        Debug.Log($"게이지 보존 효과 적용: {retentionPercent}%, 현재: {retentionComp.GetRetentionRate() * 100}%");
    }

    // 게이지 보존 효과 제거
    private void RemoveResourceRetention(PlayerClass player, float retentionPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        SpecialResourceRetentionComponent retentionComp = playerObj.GetComponent<SpecialResourceRetentionComponent>();
        if (retentionComp != null)
        {
            retentionComp.RemoveRetentionRate(retentionPercent / 100f);

            // 효과가 0이면 컴포넌트 제거 (레벨업 중이 아닐 때만)
            if (Mathf.Approximately(retentionComp.GetRetentionRate(), 0f) && !isLevelingUp)
            {
                Debug.Log("제거");
                GameObject.Destroy(retentionComp);
            }
        }
    }

    private void ApplyLightningJudgment(PlayerClass player, float damagePercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        LightningJudgmentComponent lightningComp = playerObj.GetComponent<LightningJudgmentComponent>();
        if (lightningComp == null)
        {
            lightningComp = playerObj.AddComponent<LightningJudgmentComponent>();
        }

        // 데미지 배율 설정 (퍼센트를 소수로 변환)
        lightningComp.SetDamageMultiplier(damagePercent / 100f);
        Debug.Log($"번개 심판 효과 적용: 공격력의 {damagePercent}%");
    }

    private void RemoveLightningJudgment(PlayerClass player, float damagePercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        LightningJudgmentComponent lightningComp = playerObj.GetComponent<LightningJudgmentComponent>();
        if (lightningComp != null)
        {
            lightningComp.SetDamageMultiplier(lightningComp.GetDamageMultiplier() - damagePercent / 100f);

            // 효과가 0이면 컴포넌트 제거 (레벨업 중이 아닐 때만)
            if (Mathf.Approximately(lightningComp.GetDamageMultiplier(), 0f) && !isLevelingUp)
            {
                GameObject.Destroy(lightningComp);
            }
        }
    }
}