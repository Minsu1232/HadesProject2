// MovementAbilityFactory.cs - 이동 능력 팩토리 (CSV 기반)
using System.Collections.Generic;
using UnityEngine;

public static class MovementAbilityFactory
{
    // 모든 이동 능력 생성
    public static List<MovementAbility> CreateAllMovementAbilities()
    {
        // MovementAbilityLoader가 초기화됐는지 확인
        if (MovementAbilityLoader.Instance == null)
        {
            Debug.LogError("MovementAbilityLoader가 초기화되지 않았습니다.");

            // 폴백: 하드코딩된 기본 능력 반환
            return CreateHardcodedMovementAbilities();
        }

        // CSV에서 모든 능력 생성
        return MovementAbilityLoader.Instance.CreateAllMovementAbilities();
    }

    // 특정 ID의 이동 능력 생성
    public static MovementAbility CreateMovementAbilityById(string id)
    {
        // MovementAbilityLoader가 초기화됐는지 확인
        if (MovementAbilityLoader.Instance == null)
        {
            Debug.LogError("MovementAbilityLoader가 초기화되지 않았습니다.");

            // 폴백: 하드코딩된 특정 ID의 능력 찾기
            return FindHardcodedMovementAbility(id);
        }

        // CSV에서 특정 ID의 능력 생성
        return MovementAbilityLoader.Instance.CreateMovementAbility(id);
    }

    // 폴백: 하드코딩된 기본 능력 생성 (CSV 로드 실패 시)
    private static List<MovementAbility> CreateHardcodedMovementAbilities()
    {
        Debug.Log("실패!!!!!!!");
        List<MovementAbility> abilities = new List<MovementAbility>();

        // 각 타입별로 여러 희귀도의 능력 생성

        // 대시 힘 증가 능력 (여러 희귀도)
        abilities.Add(CreateDashForceAbility(Rarity.Common, 5f, "강화된 대시", "대시 힘이 5 증가합니다. (레벨당 +1.5)"));
        abilities.Add(CreateDashForceAbility(Rarity.Uncommon, 10f, "강력한 대시", "대시 힘이 10 증가합니다. (레벨당 +3)"));
        abilities.Add(CreateDashForceAbility(Rarity.Rare, 15f, "맹렬한 대시", "대시 힘이 15 증가합니다. (레벨당 +4.5)"));
        abilities.Add(CreateDashForceAbility(Rarity.Epic, 20f, "폭발적 대시", "대시 힘이 20 증가합니다. (레벨당 +6)"));
        abilities.Add(CreateDashForceAbility(Rarity.Legendary, 30f, "번개의 질주", "대시 힘이 30 증가합니다. (레벨당 +9)"));

        // 대시 무적 능력 (여러 희귀도)
        abilities.Add(CreateDashInvincibleAbility(Rarity.Common, 0.2f, "순간 회피", "대시 중 0.2초 동안 무적 상태가 됩니다. (레벨당 +0.06초)"));
        abilities.Add(CreateDashInvincibleAbility(Rarity.Uncommon, 0.3f, "잔상 대시", "대시 중 0.3초 동안 무적 상태가 됩니다. (레벨당 +0.09초)"));
        abilities.Add(CreateDashInvincibleAbility(Rarity.Rare, 0.4f, "유령 대시", "대시 중 0.4초 동안 무적 상태가 됩니다. (레벨당 +0.12초)"));
        abilities.Add(CreateDashInvincibleAbility(Rarity.Epic, 0.5f, "차원 이동", "대시 중 0.5초 동안 무적 상태가 됩니다. (레벨당 +0.15초)"));
        abilities.Add(CreateDashInvincibleAbility(Rarity.Legendary, 0.7f, "시간 정지", "대시 중 0.7초 동안 무적 상태가 됩니다. (레벨당 +0.21초)"));

        // 대시 충격 능력 (여러 희귀도)
        abilities.Add(CreateDashImpactAbility(Rarity.Common, 10f, "충격 대시", "대시 시 적과 충돌하면 10의 피해를 입힙니다. (레벨당 +3)"));
        abilities.Add(CreateDashImpactAbility(Rarity.Uncommon, 20f, "돌진 타격", "대시 시 적과 충돌하면 20의 피해를 입힙니다. (레벨당 +6)"));
        abilities.Add(CreateDashImpactAbility(Rarity.Rare, 30f, "관통 대시", "대시 시 적과 충돌하면 30의 피해를 입힙니다. (레벨당 +9)"));
        abilities.Add(CreateDashImpactAbility(Rarity.Epic, 40f, "파괴적 돌진", "대시 시 적과 충돌하면 40의 피해를 입힙니다. (레벨당 +12)"));
        abilities.Add(CreateDashImpactAbility(Rarity.Legendary, 60f, "진격의 파괴자", "대시 시 적과 충돌하면 60의 피해를 입힙니다. (레벨당 +18)"));

        // 대시 불꽃 능력 (여러 희귀도)
        abilities.Add(CreateDashFlameAbility(Rarity.Common, 5f, "불꽃 흔적", "대시 후 불꽃이 생겨 초당 5의 피해를 입힙니다. (레벨당 +1.5)"));
        abilities.Add(CreateDashFlameAbility(Rarity.Uncommon, 10f, "화염 대시", "대시 후 불꽃이 생겨 초당 10의 피해를 입힙니다. (레벨당 +3)"));
        abilities.Add(CreateDashFlameAbility(Rarity.Rare, 15f, "맹렬한 불꽃", "대시 후 불꽃이 생겨 초당 15의 피해를 입힙니다. (레벨당 +4.5)"));
        abilities.Add(CreateDashFlameAbility(Rarity.Epic, 20f, "지옥불 자취", "대시 후 불꽃이 생겨 초당 20의 피해를 입힙니다. (레벨당 +6)"));
        abilities.Add(CreateDashFlameAbility(Rarity.Legendary, 30f, "불사조의 흔적", "대시 후 불꽃이 생겨 초당 30의 피해를 입힙니다. (레벨당 +9)"));

        return abilities;
    }

    // 대시 힘 능력 생성 도우미 메서드
    private static MovementAbility CreateDashForceAbility(Rarity rarity, float value, string name, string description)
    {
        MovementAbility ability = new MovementAbility();
        ability.Initialize(
            MovementAbility.MovementAbilityType.DashForce,
            value,
            name,
            description,
            rarity
        );
        // ID는 희귀도를 포함하여 유니크하게 생성
        ability.id = $"dash_force_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 대시 무적 능력 생성 도우미 메서드
    private static MovementAbility CreateDashInvincibleAbility(Rarity rarity, float value, string name, string description)
    {
        MovementAbility ability = new MovementAbility();
        ability.Initialize(
            MovementAbility.MovementAbilityType.DashInvincible,
            value,
            name,
            description,
            rarity
        );
        ability.id = $"dash_invincible_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 대시 충격 능력 생성 도우미 메서드
    private static MovementAbility CreateDashImpactAbility(Rarity rarity, float value, string name, string description)
    {
        MovementAbility ability = new MovementAbility();
        ability.Initialize(
            MovementAbility.MovementAbilityType.DashImpact,
            value,
            name,
            description,
            rarity
        );
        ability.id = $"dash_impact_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 대시 불꽃 능력 생성 도우미 메서드
    private static MovementAbility CreateDashFlameAbility(Rarity rarity, float value, string name, string description)
    {
        MovementAbility ability = new MovementAbility();
        ability.Initialize(
            MovementAbility.MovementAbilityType.DashFlame,
            value,
            name,
            description,
            rarity
        );
        ability.id = $"dash_flame_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 폴백: 하드코딩된 특정 ID의 능력 찾기
    private static MovementAbility FindHardcodedMovementAbility(string id)
    {
        List<MovementAbility> abilities = CreateHardcodedMovementAbilities();
        return abilities.Find(a => a.id == id);
    }
}