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
        // 1. �⺻ ���� ��ȭ (����Ʈ Ư��)
        ApplyEliteStats();
        
        // 2. ���� Ư�� �ο�
        AssignRandomAbilities(2); // 2���� Ư�� �ο�


        
    }

    private void ApplyEliteStats()
    {
        Debug.Log($"[Before] MaxHealth: {MaxHealth}, CurrentHealth: {CurrentHealth}");
        ModifyStats(
            maxHealthAmount: (int)(MaxHealth * 0.5f),     // ü�� 50% ����
            defenseAmount: (int)(CurrentDeffense * 0.3f), // ���� 30% ����
            attackAmount: (int)(CurrentAttackPower * 0.3f), // ���ݷ� 30% ����
            attackSpeedAmount: 0.2f,  // ���ݼӵ� 20% ����
            armorAmount : monsterData.armorValue // ���ͺ� ������ �������ŭ
        );
        Debug.Log($"[After] MaxHealth: {MaxHealth}, CurrentHealth: {CurrentHealth}");
    }

    private void AssignRandomAbilities(int count)
    {
        var availableAbilities = new List<IEliteAbility>
        {
            new BerserkerAbility(),    // ����ȭ - ü���� 30% ������ �� ���ݷ� ����
            new SpeedAbility(),        // �ż� - �̵��ӵ��� ���ݼӵ� �߰� ����
            new RegenerationAbility(), // ��� - �ð��� ü�� ȸ��
            new GiantAbility(),        // �Ŵ�ȭ - ũ��� ���ݹ��� ����
            new VampireAbility(),      // ���� - ���ݽ� ü�� ���
            new ShieldedAbility()      // �Ƹ� ��� - �ֱ������� �Ƹ� ȸ��
        };

        // �����ϰ� Ư�� ����
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