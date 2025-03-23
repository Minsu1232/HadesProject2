// PassiveAbility �ϼ���
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class PassiveAbility : DungeonAbility
{
    public enum PassiveType
    {
        DamageReduction,  // ���� ����
        LifeSteal,        // ����
        Counterattack,    // �ǰ� �� �ݰ�
        ItemFind          // ������ ã�� Ȯ�� ����
    }

    public PassiveType passiveType;       

    private float originalValue;         // ���� ȿ���� (������ �� ���)

    // �����ڷ� �ʱ�ȭ
    public void Initialize(PassiveType type, float value, string abilityName, string abilityDescription, Rarity abilityRarity)
    {
        passiveType = type;
        effectValue = value;
        originalValue = value;

        id = $"passive_{type.ToString().ToLower()}";
        name = abilityName;
        description = abilityDescription;
        rarity = abilityRarity;
    }

    // CSV �����ͷ� �ʱ�ȭ�ϴ� �޼��� �߰�
    public static PassiveAbility FromCSVData(Dictionary<string, string> csvData)
    {
        PassiveAbility ability = new PassiveAbility();

        // �ʼ� �� �Ľ�
        string id = csvData["ID"];
        PassiveType type = (PassiveType)System.Enum.Parse(typeof(PassiveType), csvData["Type"]);
        string name = csvData["Name"];
        string description = csvData["Description"];
        Rarity rarity = (Rarity)int.Parse(csvData["Rarity"]);
        float baseValue = float.Parse(csvData["BaseValue"]);
        // ������ ���� �Ľ� �߰�
        if (csvData.ContainsKey("LevelMultiplier") && !string.IsNullOrEmpty(csvData["LevelMultiplier"]))
        {
            ability.levelMultiplier = float.Parse(csvData["LevelMultiplier"]);
        }

        // �ɷ� �ʱ�ȭ
        ability.Initialize(type, baseValue, name, description, rarity);
        ability.id = id; // ID �� ����� (����ũ�� ID ���)
                         // ������ ��ΰ� �ִٸ� �ֵ巹����� �ε�
        if (csvData.ContainsKey("IconPath") && !string.IsNullOrEmpty(csvData["IconPath"]))
        {
            string iconAddress = csvData["IconPath"];
            Debug.Log(iconAddress);
            // �ֵ巹���� �񵿱� �ε�
            Addressables.LoadAssetAsync<Sprite>(iconAddress).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    ability.icon = handle.Result;
                    Debug.Log($"������ �ε� ����: {iconAddress}");
                }
                else
                {
                    Debug.LogWarning($"�������� �ε��� �� �����ϴ�: {iconAddress}");
                }
            };
        }
        else
        {
            Debug.LogWarning($"�������� �ε��� �� �����ϴ�");
        }

        return ability;
    }

    public override void OnAcquire(PlayerClass player)
    {
        // �нú� ȿ�� ����
        ApplyPassiveEffect(player, effectValue);

        // ����� �α�
        Debug.Log($"ȹ���� �нú� �ɷ�: {name} (Lv.{level}) - {effectValue}");
    }

    public override void OnLevelUp(PlayerClass player)
    {
        // ���� ȿ�� ����
        OnReset(player);

        // ������ �� ȿ�� ����
        level++;
        effectValue = originalValue * (1 + (level - 1) * levelMultiplier);

        // �� ȿ�� ����
        OnAcquire(player);

        // ����� �α�
        Debug.Log($"�������� �нú� �ɷ�: {name} (Lv.{level}) - {effectValue}");
    }

    public override void OnReset(PlayerClass player)
    {
        // �нú� ȿ�� ����
        RemovePassiveEffect(player, effectValue);
    }

    // �нú� ȿ�� ����
    private void ApplyPassiveEffect(PlayerClass player, float value)
    {
        switch (passiveType)
        {
            case PassiveType.DamageReduction:
                ApplyDamageReduction(player, value);
                break;
            case PassiveType.LifeSteal:
                ApplyLifeSteal(player, value);
                break;
            case PassiveType.Counterattack:
                ApplyCounterattack(player, value);
                break;
            case PassiveType.ItemFind:
                ApplyItemFind(player, value);
                break;
        }
    }

    // �нú� ȿ�� ����
    private void RemovePassiveEffect(PlayerClass player, float value)
    {
        switch (passiveType)
        {
            case PassiveType.DamageReduction:
                RemoveDamageReduction(player, value);
                break;
            case PassiveType.LifeSteal:
                RemoveLifeSteal(player, value);
                break;
            case PassiveType.Counterattack:
                RemoveCounterattack(player, value);
                break;
            case PassiveType.ItemFind:
                RemoveItemFind(player, value);
                break;
        }
    }

    // ���� ���� ȿ�� ����
    private void ApplyDamageReduction(PlayerClass player, float reductionPercent)
    {
        // damageReceiveRate ���� (���� �������� ���ذ� ����)
        float reductionFactor = reductionPercent / 100f;
        player.ModifyPower(0, 0, 0, 0, 0, 0, 0, -reductionFactor);

        Debug.Log($"���� ���� ȿ�� ����: {reductionPercent}%, ���� ���� ���: {player.PlayerStats.DamageReceiveRate}");
    }

    // ���� ���� ȿ�� ����
    private void RemoveDamageReduction(PlayerClass player, float reductionPercent)
    {       
        player.ResetPower(false, false, false, false, false, false, false, true); // ����

        Debug.Log($"���� ���� ȿ�� ����: {reductionPercent}%, ���� ���� ���: {player.PlayerStats.DamageReceiveRate}");
    }

    // ���� ȿ�� ����
    private void ApplyLifeSteal(PlayerClass player, float lifeStealPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        LifeStealComponent lifeStealComp = playerObj.GetComponent<LifeStealComponent>();
        if (lifeStealComp == null)
        {
            lifeStealComp = playerObj.AddComponent<LifeStealComponent>();
        }

        lifeStealComp.AddLifeSteal(lifeStealPercent / 100f);
        Debug.Log($"���� ȿ�� ����: {lifeStealPercent}%, ���� ������: {lifeStealComp.GetLifeStealAmount() * 100f}%");
    }

    // ���� ȿ�� ����
    private void RemoveLifeSteal(PlayerClass player, float lifeStealPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        LifeStealComponent lifeStealComp = playerObj.GetComponent<LifeStealComponent>();
        if (lifeStealComp != null)
        {
            lifeStealComp.RemoveLifeSteal(lifeStealPercent / 100f);

            // ���� ȿ���� 0�̸� ������Ʈ ����
            if (Mathf.Approximately(lifeStealComp.GetLifeStealAmount(), 0f))
            {
                GameObject.Destroy(lifeStealComp);
            }
        }
    }

    // �ǰ� �� �ݰ� ȿ�� ����
    private void ApplyCounterattack(PlayerClass player, float counterDamagePercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        CounterattackComponent counterComp = playerObj.GetComponent<CounterattackComponent>();
        if (counterComp == null)
        {
            counterComp = playerObj.AddComponent<CounterattackComponent>();
        }

        counterComp.AddCounterDamage(counterDamagePercent / 100f);
        Debug.Log($"�ݰ� ȿ�� ����: {counterDamagePercent}%, ���� �ݰ� ������: {counterComp.GetCounterDamageAmount() * 100f}%");
    }

    // �ݰ� ȿ�� ����
    private void RemoveCounterattack(PlayerClass player, float counterDamagePercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        CounterattackComponent counterComp = playerObj.GetComponent<CounterattackComponent>();
        if (counterComp != null)
        {
            counterComp.RemoveCounterDamage(counterDamagePercent / 100f);

            // �ݰ� ȿ���� 0�̸� ������Ʈ ����
            if (Mathf.Approximately(counterComp.GetCounterDamageAmount(), 0f))
            {
                GameObject.Destroy(counterComp);
            }
        }
    }

    // ������ ã�� Ȯ�� ���� ȿ�� ����
    private void ApplyItemFind(PlayerClass player, float findChanceBonus)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        ItemFindComponent itemFindComp = playerObj.GetComponent<ItemFindComponent>();
        if (itemFindComp == null)
        {
            itemFindComp = playerObj.AddComponent<ItemFindComponent>();
        }

        itemFindComp.AddItemFindBonus(findChanceBonus / 100f);
        Debug.Log($"������ ã�� ȿ�� ����: +{findChanceBonus}%, ���� ���ʽ�: +{itemFindComp.GetItemFindBonus() * 100f}%");
    }

    // ������ ã�� Ȯ�� ���� ȿ�� ����
    private void RemoveItemFind(PlayerClass player, float findChanceBonus)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        ItemFindComponent itemFindComp = playerObj.GetComponent<ItemFindComponent>();
        if (itemFindComp != null)
        {
            itemFindComp.RemoveItemFindBonus(findChanceBonus / 100f);

            // ������ ã�� ȿ���� 0�̸� ������Ʈ ����
            if (Mathf.Approximately(itemFindComp.GetItemFindBonus(), 0f))
            {
                GameObject.Destroy(itemFindComp);
            }
        }
    }
}