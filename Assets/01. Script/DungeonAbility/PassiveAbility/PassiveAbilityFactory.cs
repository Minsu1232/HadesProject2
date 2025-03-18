// 4. 패시브 능력 팩토리 (CSV 기반)
using System.Collections.Generic;
using UnityEngine;

public static class PassiveAbilityFactory
{
    // 모든 패시브 능력 생성
    public static List<PassiveAbility> CreateAllPassiveAbilities()
    {
        // PassiveAbilityLoader가 초기화되었는지 확인
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
        // PassiveAbilityLoader가 초기화되었는지 확인
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

        // 피해 감소 능력
        PassiveAbility damageReduction = new PassiveAbility();
        damageReduction.Initialize(
            PassiveAbility.PassiveType.DamageReduction,
            10f, // 10% 피해 감소
            "견고한 방어",
            "받는 피해가 10% 감소합니다. (레벨당 +3%)",
            Rarity.Common
        );
        damageReduction.id = "damage_reduction";
        abilities.Add(damageReduction);

        // 흡혈 능력
        PassiveAbility lifeSteal = new PassiveAbility();
        lifeSteal.Initialize(
            PassiveAbility.PassiveType.LifeSteal,
            5f, // 5% 흡혈
            "생명력 흡수",
            "공격 시 데미지의 5%만큼 체력을 회복합니다. (레벨당 +1.5%)",
            Rarity.Uncommon
        );
        lifeSteal.id = "life_steal";
        abilities.Add(lifeSteal);

        // 반격 능력
        PassiveAbility counterattack = new PassiveAbility();
        counterattack.Initialize(
            PassiveAbility.PassiveType.Counterattack,
            15f, // 15% 반격 데미지
            "가시 갑옷",
            "피격 시 받은 데미지의 15%를 공격자에게 반사합니다. (레벨당 +4.5%)",
            Rarity.Rare
        );
        counterattack.id = "counterattack";
        abilities.Add(counterattack);

        // 아이템 찾기 능력
        PassiveAbility itemFind = new PassiveAbility();
        itemFind.Initialize(
            PassiveAbility.PassiveType.ItemFind,
            20f, // 20% 아이템 찾기 보너스
            "행운의 손길",
            "아이템 드롭 확률이 20% 증가합니다. (레벨당 +6%)",
            Rarity.Uncommon
        );
        itemFind.id = "item_find";
        abilities.Add(itemFind);

        return abilities;
    }

    // 폴백: 하드코딩된 특정 ID의 능력 찾기
    private static PassiveAbility FindHardcodedPassiveAbility(string id)
    {
        List<PassiveAbility> abilities = CreateHardcodedPassiveAbilities();
        return abilities.Find(a => a.id == id);
    }
}