using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class EliteMonster : MonsterClass
{
    private List<IEliteAbility> eliteAbilities = new List<IEliteAbility>();

    public EliteMonster(ICreatureData data) : base(data)
    {
        InitializeEliteMonster();
    }

    private void InitializeEliteMonster()
    {
        // 1. 기본 스탯 강화 (엘리트 특성)
        ApplyEliteStats();
        
        // 2. 랜덤 특성 부여
        AssignRandomAbilities(2); // 2개의 특성 부여


        
    }

    private void ApplyEliteStats()
    {
        Debug.Log($"[Before] MaxHealth: {MaxHealth}, CurrentHealth: {CurrentHealth}");
        ModifyStats(
            maxHealthAmount: (int)(MaxHealth * 0.5f),     // 체력 50% 증가
            defenseAmount: (int)(CurrentDeffense * 0.3f), // 방어력 30% 증가
            attackAmount: (int)(CurrentAttackPower * 0.3f), // 공격력 30% 증가
            attackSpeedAmount: 0.2f,  // 공격속도 20% 증가
            armorAmount : monsterData.armorValue // 몬스터별 지정한 밸류값만큼
        );
        Debug.Log($"[After] MaxHealth: {MaxHealth}, CurrentHealth: {CurrentHealth}");
    }

    private void AssignRandomAbilities(int count)
    {
        var availableAbilities = new List<IEliteAbility>
        {
            new BerserkerAbility(),    // 광폭화 - 체력이 30% 이하일 때 공격력 증가
            new SpeedAbility(),        // 신속 - 이동속도와 공격속도 추가 증가
            new RegenerationAbility(), // 재생 - 시간당 체력 회복
            new GiantAbility(),        // 거대화 - 크기와 공격범위 증가
            new VampireAbility(),      // 흡혈 - 공격시 체력 흡수
            new ShieldedAbility()      // 아머 재생 - 주기적으로 아머 회복
        };

        // 랜덤하게 특성 선택
        for (int i = 0; i < count; i++)
        {
            if (availableAbilities.Count > 0)
            {
                int randomIndex = Random.Range(0, availableAbilities.Count);
                eliteAbilities.Add(availableAbilities[randomIndex]);
                availableAbilities.RemoveAt(randomIndex);
            }
        }
    }

    public List<IEliteAbility> GetEliteAbilities()
    {
        return eliteAbilities;
    }
}