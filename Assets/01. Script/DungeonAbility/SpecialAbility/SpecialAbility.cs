// SpecialAbility.cs - Ư�� ��ų ���� �ɷ� Ŭ���� (DungeonAbility ���)
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpecialAbility : DungeonAbility
{
    public enum SpecialAbilityType
    {
        ResourceRetention,// ��ų ��� �� ������ �Ϻ� ����
        LightningJudgment // ��� ������ ���� ����
        // ���� �ٸ� Ư�� ��ų ���� �ɷµ� �߰� ����
    }

    public SpecialAbilityType specialType;
    private float originalValue;

    // �����ڷ� �ʱ�ȭ
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

    // CSV �����ͷ� �ʱ�ȭ�ϴ� �޼��� �߰�
    public static SpecialAbility FromCSVData(Dictionary<string, string> csvData)
    {
        SpecialAbility ability = new SpecialAbility();

        // �ʼ� �� �Ľ�
        string id = csvData["ID"];
        SpecialAbilityType type = (SpecialAbilityType)System.Enum.Parse(typeof(SpecialAbilityType), csvData["Type"]);
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

        // ������ ��ΰ� �ִٸ� ��巹����� �ε�
        if (csvData.ContainsKey("IconPath") && !string.IsNullOrEmpty(csvData["IconPath"]))
        {
            // ��巹���� �ε� �ڵ� (���� �ڵ�� ����)
        }

        return ability;
    }

    public override void OnAcquire(PlayerClass player)
    {
        // Ư�� ȿ�� ����
        ApplySpecialEffect(player, effectValue);
        Debug.Log($"ȹ���� Ư�� ��ų �ɷ�: {name} (Lv.{level}) - {effectValue}");
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
        Debug.Log($"�������� Ư�� ��ų �ɷ�: {name} (Lv.{level}) - {effectValue}");
    }

    public override void OnReset(PlayerClass player)
    {
        // Ư�� ȿ�� ����
        RemoveSpecialEffect(player, effectValue);
    }

    // Ư�� ȿ�� ����
    private void ApplySpecialEffect(PlayerClass player, float value)
    {
        switch (specialType)
        {
            case SpecialAbilityType.ResourceRetention:
                ApplyResourceRetention(player, value);
                break;
            case SpecialAbilityType.LightningJudgment:
                ApplyLightningJudgment(player, value);
                break;
                // ���� �ٸ� ���̽� �߰�
        }
    }

    // Ư�� ȿ�� ����
    private void RemoveSpecialEffect(PlayerClass player, float value)
    {
        switch (specialType)
        {
            case SpecialAbilityType.ResourceRetention:
                RemoveResourceRetention(player, value);
                break;
            case SpecialAbilityType.LightningJudgment:
                RemoveLightningJudgment(player, value);
                break;
                // ���� �ٸ� ���̽� �߰�
        }
    }

    // ������ ���� ȿ�� ����
    private void ApplyResourceRetention(PlayerClass player, float retentionPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        SpecialResourceRetentionComponent retentionComp = playerObj.GetComponent<SpecialResourceRetentionComponent>();
        if (retentionComp == null)
        {
            retentionComp = playerObj.AddComponent<SpecialResourceRetentionComponent>();
        }

        retentionComp.AddRetentionRate(retentionPercent / 100f);
        Debug.Log($"������ ���� ȿ�� ����: {retentionPercent}%, ����: {retentionComp.GetRetentionRate() * 100}%");
    }

    // ������ ���� ȿ�� ����
    private void RemoveResourceRetention(PlayerClass player, float retentionPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        SpecialResourceRetentionComponent retentionComp = playerObj.GetComponent<SpecialResourceRetentionComponent>();
        if (retentionComp != null)
        {
            retentionComp.RemoveRetentionRate(retentionPercent / 100f);

            // ȿ���� 0�̸� ������Ʈ ����
            if (Mathf.Approximately(retentionComp.GetRetentionRate(), 0f))
            {
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

        // ������ ���� ���� (�ۼ�Ʈ�� �Ҽ��� ��ȯ)
        lightningComp.SetDamageMultiplier(damagePercent / 100f);
        Debug.Log($"���� ���� ȿ�� ����: ���ݷ��� {damagePercent}%");
    }

    // SpecialAbility.cs�� RemoveSpecialEffect �޼��忡 �߰�
    private void RemoveLightningJudgment(PlayerClass player, float damagePercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        LightningJudgmentComponent lightningComp = playerObj.GetComponent<LightningJudgmentComponent>();
        if (lightningComp != null)
        {
            lightningComp.SetDamageMultiplier(lightningComp.GetDamageMultiplier() - damagePercent / 100f);

            // ȿ���� 0�̸� ������Ʈ ����
            if (Mathf.Approximately(lightningComp.GetDamageMultiplier(), 0f))
            {
                GameObject.Destroy(lightningComp);
            }
        }
    }
}