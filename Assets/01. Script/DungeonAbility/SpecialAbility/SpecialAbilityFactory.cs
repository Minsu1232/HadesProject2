// SpecialAbilityFactory.cs - 특수 능력 팩토리 (CSV 기반)
using System.Collections.Generic;
using UnityEngine;

public static class SpecialAbilityFactory
{
    // 모든 특수 능력 생성
    public static List<SpecialAbility> CreateAllSpecialAbilities()
    {
        // SpecialAbilityLoader가 초기화됐는지 확인
        if (SpecialAbilityLoader.Instance == null)
        {
            Debug.LogError("SpecialAbilityLoader가 초기화되지 않았습니다.");

            // 폴백: 하드코딩된 기본 능력 반환
            return CreateHardcodedSpecialAbilities();
        }

        // CSV에서 모든 능력 생성
        return SpecialAbilityLoader.Instance.CreateAllSpecialAbilities();
    }

    // 특정 ID의 특수 능력 생성
    public static SpecialAbility CreateSpecialAbilityById(string id)
    {
        // SpecialAbilityLoader가 초기화됐는지 확인
        if (SpecialAbilityLoader.Instance == null)
        {
            Debug.LogError("SpecialAbilityLoader가 초기화되지 않았습니다.");

            // 폴백: 하드코딩된 특정 ID의 능력 찾기
            return FindHardcodedSpecialAbility(id);
        }

        // CSV에서 특정 ID의 능력 생성
        return SpecialAbilityLoader.Instance.CreateSpecialAbility(id);
    }

    // 폴백: 하드코딩된 기본 능력 생성 (CSV 로드 실패 시)
    private static List<SpecialAbility> CreateHardcodedSpecialAbilities()
    {
        List<SpecialAbility> abilities = new List<SpecialAbility>();

        // 각 타입별로 여러 희귀도의 능력 생성

        // 게이지 보존 능력 (여러 희귀도)
        abilities.Add(CreateResourceRetentionAbility(Rarity.Common, 10f, "기본 보존", "특수 스킬 사용 후 게이지가 10% 보존됩니다. (레벨당 +3%)"));
        abilities.Add(CreateResourceRetentionAbility(Rarity.Uncommon, 20f, "에너지 보존", "특수 스킬 사용 후 게이지가 20% 보존됩니다. (레벨당 +6%)"));
        abilities.Add(CreateResourceRetentionAbility(Rarity.Rare, 30f, "효율적 사용", "특수 스킬 사용 후 게이지가 30% 보존됩니다. (레벨당 +9%)"));
        abilities.Add(CreateResourceRetentionAbility(Rarity.Epic, 40f, "에너지 순환", "특수 스킬 사용 후 게이지가 40% 보존됩니다. (레벨당 +12%)"));
        abilities.Add(CreateResourceRetentionAbility(Rarity.Legendary, 50f, "영구 에너지", "특수 스킬 사용 후 게이지가 50% 보존됩니다. (레벨당 +15%)"));

        return abilities;
    }

    // 게이지 보존 능력 생성 도우미 메서드
    private static SpecialAbility CreateResourceRetentionAbility(Rarity rarity, float value, string name, string description)
    {
        SpecialAbility ability = new SpecialAbility();
        ability.Initialize(
            SpecialAbility.SpecialAbilityType.ResourceRetention,
            value,
            name,
            description,
            rarity
        );
        // ID는 희귀도를 포함하여 유니크하게 생성
        ability.id = $"resource_retention_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 폴백: 하드코딩된 특정 ID의 능력 찾기
    private static SpecialAbility FindHardcodedSpecialAbility(string id)
    {
        List<SpecialAbility> abilities = CreateHardcodedSpecialAbilities();
        return abilities.Find(a => a.id == id);
    }
}