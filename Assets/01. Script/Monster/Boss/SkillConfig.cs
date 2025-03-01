using UnityEngine;

/// <summary>
/// ��ų ������ �����ϴ� Ŭ����
/// BossSkillConfigs.csv���� �ε�� ������ �����մϴ�.
/// </summary>
[System.Serializable]
public class SkillConfig
{
    public int configId;                    // ���� ID
    public SkillStrategyType strategyType;  // ��ų ���� Ÿ��
    public SkillEffectType effectType;      // ��ų ����Ʈ Ÿ��
    public ProjectileMovementType moveType; // �߻�ü ������ Ÿ��
    public ProjectileImpactType impactType; // �߻�ü �浹 ����Ʈ Ÿ��
    public string configName;               // ���� �̸�
    public float damageMultiplier = 1.0f; // �⺻���� 1.0
    // ���� ���� ���� (���� ��ų�� ��츸 ���)
    public string buffTypes;                // ���� Ÿ�� (������ �����ڷ� ���� �� ����)
    public string buffDurations;            // ���� ���ӽð� (������ �����ڷ� ���� �� ����)
    public string buffValues;               // ���� ��ġ�� (������ �����ڷ� ���� �� ����)

    public SkillConfig(int id, string name, SkillStrategyType strategy, SkillEffectType effect,
                      ProjectileMovementType move, ProjectileImpactType impact,
                      string buffs = "", string durations = "", string values = "")
    {
        configId = id;
        configName = name;
        strategyType = strategy;
        effectType = effect;
        moveType = move;
        impactType = impact;
        buffTypes = buffs;
        buffDurations = durations;
        buffValues = values;
    }
}