// 4. �нú� �ɷ� ���丮 (CSV ���)
using System.Collections.Generic;
using UnityEngine;

public static class PassiveAbilityFactory
{
    // ��� �нú� �ɷ� ����
    public static List<PassiveAbility> CreateAllPassiveAbilities()
    {
        // PassiveAbilityLoader�� �ʱ�ȭ�Ǿ����� Ȯ��
        if (PassiveAbilityLoader.Instance == null)
        {
            Debug.LogError("PassiveAbilityLoader�� �ʱ�ȭ���� �ʾҽ��ϴ�.");

            // ����: �ϵ��ڵ��� �⺻ �ɷ� ��ȯ
            return CreateHardcodedPassiveAbilities();
        }

        // CSV���� ��� �ɷ� ����
        return PassiveAbilityLoader.Instance.CreateAllPassiveAbilities();
    }

    // Ư�� ID�� �нú� �ɷ� ����
    public static PassiveAbility CreatePassiveAbilityById(string id)
    {
        // PassiveAbilityLoader�� �ʱ�ȭ�Ǿ����� Ȯ��
        if (PassiveAbilityLoader.Instance == null)
        {
            Debug.LogError("PassiveAbilityLoader�� �ʱ�ȭ���� �ʾҽ��ϴ�.");

            // ����: �ϵ��ڵ��� �⺻ �ɷ� ã��
            return FindHardcodedPassiveAbility(id);
        }

        // CSV���� Ư�� ID�� �ɷ� ����
        return PassiveAbilityLoader.Instance.CreatePassiveAbility(id);
    }

    // ����: �ϵ��ڵ��� �⺻ �ɷ� ���� (CSV �ε� ���� ��)
    private static List<PassiveAbility> CreateHardcodedPassiveAbilities()
    {
        List<PassiveAbility> abilities = new List<PassiveAbility>();

        // ���� ���� �ɷ�
        PassiveAbility damageReduction = new PassiveAbility();
        damageReduction.Initialize(
            PassiveAbility.PassiveType.DamageReduction,
            10f, // 10% ���� ����
            "�߰��� ���",
            "�޴� ���ذ� 10% �����մϴ�. (������ +3%)",
            Rarity.Common
        );
        damageReduction.id = "damage_reduction";
        abilities.Add(damageReduction);

        // ���� �ɷ�
        PassiveAbility lifeSteal = new PassiveAbility();
        lifeSteal.Initialize(
            PassiveAbility.PassiveType.LifeSteal,
            5f, // 5% ����
            "����� ���",
            "���� �� �������� 5%��ŭ ü���� ȸ���մϴ�. (������ +1.5%)",
            Rarity.Uncommon
        );
        lifeSteal.id = "life_steal";
        abilities.Add(lifeSteal);

        // �ݰ� �ɷ�
        PassiveAbility counterattack = new PassiveAbility();
        counterattack.Initialize(
            PassiveAbility.PassiveType.Counterattack,
            15f, // 15% �ݰ� ������
            "���� ����",
            "�ǰ� �� ���� �������� 15%�� �����ڿ��� �ݻ��մϴ�. (������ +4.5%)",
            Rarity.Rare
        );
        counterattack.id = "counterattack";
        abilities.Add(counterattack);

        // ������ ã�� �ɷ�
        PassiveAbility itemFind = new PassiveAbility();
        itemFind.Initialize(
            PassiveAbility.PassiveType.ItemFind,
            20f, // 20% ������ ã�� ���ʽ�
            "����� �ձ�",
            "������ ��� Ȯ���� 20% �����մϴ�. (������ +6%)",
            Rarity.Uncommon
        );
        itemFind.id = "item_find";
        abilities.Add(itemFind);

        return abilities;
    }

    // ����: �ϵ��ڵ��� Ư�� ID�� �ɷ� ã��
    private static PassiveAbility FindHardcodedPassiveAbility(string id)
    {
        List<PassiveAbility> abilities = CreateHardcodedPassiveAbilities();
        return abilities.Find(a => a.id == id);
    }
}