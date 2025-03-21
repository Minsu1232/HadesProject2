// AttackAbilityFactory.cs - 공격 능력 팩토리 (CSV 기반)
using System.Collections.Generic;
using UnityEngine;

public static class AttackAbilityFactory
{
    // 모든 공격 능력 생성
    public static List<AttackAbility> CreateAllAttackAbilities()
    {
        // AttackAbilityLoader가 초기화됐는지 확인
        if (AttackAbilityLoader.Instance == null)
        {
            Debug.LogError("AttackAbilityLoader가 초기화되지 않았습니다.");

            // 폴백: 하드코딩된 기본 능력 반환
            return CreateHardcodedAttackAbilities();
        }

        // CSV에서 모든 능력 생성
        return AttackAbilityLoader.Instance.CreateAllAttackAbilities();
    }

    // 특정 ID의 공격 능력 생성
    public static AttackAbility CreateAttackAbilityById(string id)
    {
        // AttackAbilityLoader가 초기화됐는지 확인
        if (AttackAbilityLoader.Instance == null)
        {
            Debug.LogError("AttackAbilityLoader가 초기화되지 않았습니다.");

            // 폴백: 하드코딩된 기본 능력 찾기
            return FindHardcodedAttackAbility(id);
        }

        // CSV에서 특정 ID의 능력 생성
        return AttackAbilityLoader.Instance.CreateAttackAbility(id);
    }

    // 폴백: 하드코딩된 기본 능력 생성 (CSV 로드 실패 시)
    private static List<AttackAbility> CreateHardcodedAttackAbilities()
    {
        List<AttackAbility> abilities = new List<AttackAbility>();

        // 각 타입별로 여러 희귀도의 능력 생성

        //콤보 강화 능력(여러 희귀도)
        //abilities.Add(CreateComboEnhancementAbility(Rarity.Common, 15f, "기본 콤보", "연속 공격 시 콤보 데미지가 15% 증가합니다. (레벨당 +4.5%)"));
        //abilities.Add(CreateComboEnhancementAbility(Rarity.Uncommon, 25f, "향상된 콤보", "연속 공격 시 콤보 데미지가 25% 증가합니다. (레벨당 +7.5%)"));
        //abilities.Add(CreateComboEnhancementAbility(Rarity.Rare, 35f, "강력한 콤보", "연속 공격 시 콤보 데미지가 35% 증가합니다. (레벨당 +10.5%)"));
        //abilities.Add(CreateComboEnhancementAbility(Rarity.Epic, 45f, "파괴적 콤보", "연속 공격 시 콤보 데미지가 45% 증가합니다. (레벨당 +13.5%)"));
        //abilities.Add(CreateComboEnhancementAbility(Rarity.Legendary, 60f, "무한 연속타", "연속 공격 시 콤보 데미지가 60% 증가합니다. (레벨당 +18%)"));

        //약자 공격 능력(여러 희귀도)
        //abilities.Add(CreateWeakPreyAbility(Rarity.Common, 15f, "약점 타격", "체력이 30% 이하인 적에게 15% 추가 피해를 줍니다. (레벨당 +4.5%)"));
        //abilities.Add(CreateWeakPreyAbility(Rarity.Uncommon, 25f, "치명적 약점", "체력이 30% 이하인 적에게 25% 추가 피해를 줍니다. (레벨당 +7.5%)"));
        //abilities.Add(CreateWeakPreyAbility(Rarity.Rare, 35f, "약자 포착", "체력이 35% 이하인 적에게 35% 추가 피해를 줍니다. (레벨당 +10.5%)"));
        //abilities.Add(CreateWeakPreyAbility(Rarity.Epic, 45f, "사냥꾼의 눈", "체력이 40% 이하인 적에게 45% 추가 피해를 줍니다. (레벨당 +13.5%)"));
        //abilities.Add(CreateWeakPreyAbility(Rarity.Legendary, 60f, "약자 멸시", "체력이 50% 이하인 적에게 60% 추가 피해를 줍니다. (레벨당 +18%)"));

        //연쇄 타격 능력(여러 희귀도)
        //abilities.Add(CreateChainStrikeAbility(Rarity.Common, 10f, "이중 타격", "공격 시 10% 확률로 추가 타격을 가합니다. (레벨당 +3%)"));
        //abilities.Add(CreateChainStrikeAbility(Rarity.Uncommon, 15f, "연속 타격", "공격 시 15% 확률로 추가 타격을 가합니다. (레벨당 +4.5%)"));
        //abilities.Add(CreateChainStrikeAbility(Rarity.Rare, 20f, "연쇄 공격", "공격 시 20% 확률로 추가 타격을 가합니다. (레벨당 +6%)"));
        //abilities.Add(CreateChainStrikeAbility(Rarity.Epic, 25f, "공격의 폭풍", "공격 시 25% 확률로 추가 타격을 가합니다. (레벨당 +7.5%)"));
        //abilities.Add(CreateChainStrikeAbility(Rarity.Legendary, 30f, "무한 연쇄", "공격 시 30% 확률로 추가 타격을 가합니다. (레벨당 +9%)"));

        //게이지 강화 능력(여러 희귀도)
        //abilities.Add(CreateGaugeBoostAbility(Rarity.Common, 15f, "기본 충전", "무기 게이지 충전량이 15% 증가합니다. (레벨당 +4.5%)"));
        //abilities.Add(CreateGaugeBoostAbility(Rarity.Uncommon, 25f, "빠른 충전", "무기 게이지 충전량이 25% 증가합니다. (레벨당 +7.5%)"));
        //abilities.Add(CreateGaugeBoostAbility(Rarity.Rare, 35f, "향상된 충전", "무기 게이지 충전량이 35% 증가합니다. (레벨당 +10.5%)"));
        //abilities.Add(CreateGaugeBoostAbility(Rarity.Epic, 45f, "급속 충전", "무기 게이지 충전량이 45% 증가합니다. (레벨당 +13.5%)"));
        //abilities.Add(CreateGaugeBoostAbility(Rarity.Legendary, 60f, "초충전", "무기 게이지 충전량이 60% 증가합니다. (레벨당 +18%)"));

        //방어력 파괴 능력(여러 희귀도)
        //abilities.Add(CreateArmorCrushAbility(Rarity.Common, 5f, "방어력 감소", "공격 시 적의 방어력을 5% 감소시킵니다. (레벨당 +1.5%)"));
        //abilities.Add(CreateArmorCrushAbility(Rarity.Uncommon, 10f, "방어구 파괴", "공격 시 적의 방어력을 10% 감소시킵니다. (레벨당 +3%)"));
        //abilities.Add(CreateArmorCrushAbility(Rarity.Rare, 15f, "방어구 분쇄", "공격 시 적의 방어력을 15% 감소시킵니다. (레벨당 +4.5%)"));
        //abilities.Add(CreateArmorCrushAbility(Rarity.Epic, 20f, "관통 공격", "공격 시 적의 방어력을 20% 감소시킵니다. (레벨당 +6%)"));
        //abilities.Add(CreateArmorCrushAbility(Rarity.Legendary, 25f, "완전 관통", "공격 시 적의 방어력을 25% 감소시킵니다. (레벨당 +7.5%)"));

        return abilities;
    }

    // 콤보 강화 능력 생성 도우미 메서드
    private static AttackAbility CreateComboEnhancementAbility(Rarity rarity, float value, string name, string description)
    {
        AttackAbility ability = new AttackAbility();
        ability.Initialize(
            AttackAbility.AttackAbilityType.ComboEnhancement,
            value,
            name,
            description,
            rarity
        );
        // ID는 희귀도를 포함하여 유니크하게 생성
        ability.id = $"combo_enhancement_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 약자 공격 능력 생성 도우미 메서드
    private static AttackAbility CreateWeakPreyAbility(Rarity rarity, float value, string name, string description)
    {
        AttackAbility ability = new AttackAbility();
        ability.Initialize(
            AttackAbility.AttackAbilityType.WeakPrey,
            value,
            name,
            description,
            rarity
        );
        ability.id = $"weak_prey_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 연쇄 타격 능력 생성 도우미 메서드
    private static AttackAbility CreateChainStrikeAbility(Rarity rarity, float value, string name, string description)
    {
        AttackAbility ability = new AttackAbility();
        ability.Initialize(
            AttackAbility.AttackAbilityType.ChainStrike,
            value,
            name,
            description,
            rarity
        );
        ability.id = $"chain_strike_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 게이지 강화 능력 생성 도우미 메서드
    private static AttackAbility CreateGaugeBoostAbility(Rarity rarity, float value, string name, string description)
    {
        AttackAbility ability = new AttackAbility();
        ability.Initialize(
            AttackAbility.AttackAbilityType.GaugeBoost,
            value,
            name,
            description,
            rarity
        );
        ability.id = $"gauge_boost_{rarity.ToString().ToLower()}";
        return ability;
    }

    // 방어력 파괴 능력 생성 도우미 메서드
    private static AttackAbility CreateArmorCrushAbility(Rarity rarity, float value, string name, string description)
    {
        AttackAbility ability = new AttackAbility();
        ability.Initialize(
            AttackAbility.AttackAbilityType.ArmorCrush,
            value,
            name,
            description,
            rarity
        );
        ability.id = $"armor_crush_{rarity.ToString().ToLower()}";
        
        return ability;
    }

    // 폴백: 하드코딩된 특정 ID의 능력 찾기
    private static AttackAbility FindHardcodedAttackAbility(string id)
    {
        List<AttackAbility> abilities = CreateHardcodedAttackAbilities();
        return abilities.Find(a => a.id == id);
    }
}