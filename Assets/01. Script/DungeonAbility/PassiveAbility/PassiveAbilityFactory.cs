// 패시브 능력 팩토리 (CSV 기반)
using System.Collections.Generic;
using UnityEngine;

public static class PassiveAbilityFactory
{
    // 모든 패시브 능력 생성
    public static List<PassiveAbility> CreateAllPassiveAbilities()
    {
        // PassiveAbilityLoader가 초기화됐는지 확인
        if (PassiveAbilityLoader.Instance == null)
        {
            Debug.LogError("PassiveAbilityLoader가 초기화되지 않았습니다.");

            // 폴백: 하드코딩된 기본 능력 반환
            return CreateHardcodedPassiveAbilities();
        }

        // CSV에서 모든 능력 생성
        return PassiveAbilityLoader.Instance.CreateAllPassiveAbilities();
    }

    // 특정 ID의 패시브 능력 생성
    public static PassiveAbility CreatePassiveAbilityById(string id)
    {
        // PassiveAbilityLoader가 초기화됐는지 확인
        if (PassiveAbilityLoader.Instance == null)
        {
            Debug.LogError("PassiveAbilityLoader가 초기화되지 않았습니다.");

            // 폴백: 하드코딩된 기본 능력 찾기
            return FindHardcodedPassiveAbility(id);
        }

        // CSV에서 특정 ID의 능력 생성
        return PassiveAbilityLoader.Instance.CreatePassiveAbility(id);
    }

    // 폴백: 하드코딩된 기본 능력 생성 (CSV 로드 실패 시)
    private static List<PassiveAbility> CreateHardcodedPassiveAbilities()
    {
        List<PassiveAbility> abilities = new List<PassiveAbility>();

        // 각 타입별로 여러 희귀도의 능력 생성

        // 피해 감소 능력 (여러 희귀도)
        abilities.Add(CreateDamageReductionAbility(Rarity.Common, 5f, "기본 방어", "받는 피해가 5% 감소합니다. (레벨당 +1.5%)"));
        abilities.Add(CreateDamageReductionAbility(Rarity.Uncommon, 10f, "강건한 방어", "받는 피해가 10% 감소합니다. (레벨당 +3%)"));
        abilities.Add(CreateDamageReductionAbility(Rarity.Rare, 15f, "철벽 방어", "받는 피해가 15% 감소합니다. (레벨당 +4.5%)"));
        abilities.Add(CreateDamageReductionAbility(Rarity.Epic, 20f, "불가침 방어", "받는 피해가 20% 감소합니다. (레벨당 +6%)"));
        abilities.Add(CreateDamageReductionAbility(Rarity.Legendary, 25f, "신의 방패", "받는 피해가 25% 감소합니다. (레벨당 +7.5%)"));

        // 흡혈 능력 (여러 희귀도)
        abilities.Add(CreateLifeStealAbility(Rarity.Common, 3f, "피의 갈증", "공격 시 데미지의 3%만큼 체력을 회복합니다. (레벨당 +0.9%)"));
        abilities.Add(CreateLifeStealAbility(Rarity.Uncommon, 5f, "생명력 흡수", "공격 시 데미지의 5%만큼 체력을 회복합니다. (레벨당 +1.5%)"));
        abilities.Add(CreateLifeStealAbility(Rarity.Rare, 8f, "피의 계약", "공격 시 데미지의 8%만큼 체력을 회복합니다. (레벨당 +2.4%)"));
        abilities.Add(CreateLifeStealAbility(Rarity.Epic, 12f, "피의 의식", "공격 시 데미지의 12%만큼 체력을 회복합니다. (레벨당 +3.6%)"));
        abilities.Add(CreateLifeStealAbility(Rarity.Legendary, 15f, "불멸의 흡혈", "공격 시 데미지의 15%만큼 체력을 회복합니다. (레벨당 +4.5%)"));

        // 반격 능력 (여러 희귀도)
        abilities.Add(CreateCounterattackAbility(Rarity.Common, 10f, "기본 반격", "피격 시 받은 데미지의 10%를 공격자에게 반사합니다. (레벨당 +3%)"));
        abilities.Add(CreateCounterattackAbility(Rarity.Uncommon, 15f, "가시 갑옷", "피격 시 받은 데미지의 15%를 공격자에게 반사합니다. (레벨당 +4.5%)"));
        abilities.Add(CreateCounterattackAbility(Rarity.Rare, 20f, "복수의 갑옷", "피격 시 받은 데미지의 20%를 공격자에게 반사합니다. (레벨당 +6%)"));
        abilities.Add(CreateCounterattackAbility(Rarity.Epic, 25f, "원한의 껍질", "피격 시 받은 데미지의 25%를 공격자에게 반사합니다. (레벨당 +7.5%)"));
        abilities.Add(CreateCounterattackAbility(Rarity.Legendary, 30f, "심판의 반사", "피격 시 받은 데미지의 30%를 공격자에게 반사합니다. (레벨당 +9%)"));

        // 아이템 찾기 능력 (여러 희귀도)
        abilities.Add(CreateItemFindAbility(Rarity.Common, 10f, "행운의 기운", "아이템 드롭 확률이 10% 증가합니다. (레벨당 +3%)"));
        abilities.Add(CreateItemFindAbility(Rarity.Uncommon, 20f, "행운의 손길", "아이템 드롭 확률이 20% 증가합니다. (레벨당 +6%)"));
        abilities.Add(CreateItemFindAbility(Rarity.Rare, 30f, "보물 사냥꾼", "아이템 드롭 확률이 30% 증가합니다. (레벨당 +9%)"));
        abilities.Add(CreateItemFindAbility(Rarity.Epic, 40f, "황금의 손", "아이템 드롭 확률이 40% 증가합니다. (레벨당 +12%)"));
        abilities.Add(CreateItemFindAbility(Rarity.Legendary, 50f, "만물의 발견자", "아이템 드롭 확률이 50% 증가합니다. (레벨당 +15%)"));

        return abilities;
    }

    // 피해 감소 능력 생성 도우미 메서드
    private static PassiveAbility CreateDamageReductionAbility(Rarity rarity, float value, string name, string description)
    {
        PassiveAbility ability = new PassiveAbility();
        ability.Initialize(
            PassiveAbility.PassiveType.DamageReduction,
            value,
            name,
            description,
            rarity
        );
        // ID는 희귀도를 포함하여 유니크하게 생성
        ability.id = $"damage_reduction_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 흡혈 능력 생성 도우미 메서드
    private static PassiveAbility CreateLifeStealAbility(Rarity rarity, float value, string name, string description)
    {
        PassiveAbility ability = new PassiveAbility();
        ability.Initialize(
            PassiveAbility.PassiveType.LifeSteal,
            value,
            name,
            description,
            rarity
        );
        ability.id = $"life_steal_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 반격 능력 생성 도우미 메서드
    private static PassiveAbility CreateCounterattackAbility(Rarity rarity, float value, string name, string description)
    {
        PassiveAbility ability = new PassiveAbility();
        ability.Initialize(
            PassiveAbility.PassiveType.Counterattack,
            value,
            name,
            description,
            rarity
        );
        ability.id = $"counterattack_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 아이템 찾기 능력 생성 도우미 메서드
    private static PassiveAbility CreateItemFindAbility(Rarity rarity, float value, string name, string description)
    {
        PassiveAbility ability = new PassiveAbility();
        ability.Initialize(
            PassiveAbility.PassiveType.ItemFind,
            value,
            name,
            description,
            rarity
        );
        ability.id = $"item_find_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 폴백: 하드코딩된 특정 ID의 능력 찾기
    private static PassiveAbility FindHardcodedPassiveAbility(string id)
    {
        List<PassiveAbility> abilities = CreateHardcodedPassiveAbilities();
        return abilities.Find(a => a.id == id);
    }
}