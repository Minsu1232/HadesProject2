using System;
using UnityEngine;

/// <summary>
/// ��ų ID�� ���� ������� ��ų ���� ��ü�� �����ϴ� ���丮 Ŭ����
/// </summary>
public static class SkillStrategyFactory
{
    /// <summary>
    /// ��ų ���� ID�� ������� ��ų ���� ��ü�� �����մϴ�.
    /// </summary>
    /// <param name="configId">��ų ���� ID</param>
    /// <param name="owner">������ AI</param>
    /// <param name="data">���� ������</param>
    /// <returns>������ ��ų ���� ��ü</returns>
    public static ISkillStrategy CreateFromConfig(int configId, CreatureAI owner, ICreatureData data)
    {
        var config = SkillConfigManager.Instance.GetSkillConfig(configId);
        if (config == null)
        {
            Debug.LogError($"��ų ������ ã�� �� ����: ID {configId}");
            return null;
        }

        Debug.Log($"[SkillStrategyFactory] ��ų ���� ���� ����: {config.configName} (ID: {configId})");

        try
        {
            // ��ų ���� ����
            ISkillStrategy skillStrategy = StrategyFactory.CreateSkillStrategy(config.strategyType, owner);
            if (skillStrategy == null)
            {
                Debug.LogError($"[SkillStrategyFactory] ��ų ���� ���� ����: {config.strategyType}");
                return null;
            }

            // StrategyFactory�� �̵� ���� ���� ���� (���Ǳ��� moveType ���)
            IProjectileMovement moveStrategy = StrategyFactory.CreateProjectileMovement(config.moveType, data);
            IProjectileImpact impactEffect = StrategyFactory.CreateProjectileImpact(config.impactType, data);

            // Ŀ���� SkillEffect ���� - �߿� ����: ���� ProjectileSkillEffect ����
            ISkillEffect skillEffect = null;


                // �ٸ� ����Ʈ Ÿ���� ���� StrategyFactory Ȱ��
                skillEffect = StrategyFactory.CreateSkillEffect(config.effectType, data, owner);
            

            if (skillEffect == null)
            {
                Debug.LogError($"[SkillStrategyFactory] ��ų ����Ʈ ���� ����: {config.effectType}");
                return null;
            }

            // ������Ʈ ������ �ʿ��� �����̸� ����
            if (skillStrategy is ISkillStrategyComponentInjection injectionStrategy)
            {
                Debug.Log($"[SkillStrategyFactory] ������Ʈ ����: MoveType={config.moveType}");
                injectionStrategy.SetSkillEffect(skillEffect);
                injectionStrategy.SetProjectileMovement(moveStrategy);
                injectionStrategy.SetProjectileImpact(impactEffect);
            }

            // ���� ��ų ó��
            if (config.strategyType == SkillStrategyType.Buff && data is BossData bossData)
            {
                SetupBuffData(config, bossData);
            }

            // ��ų �ʱ�ȭ (������Ʈ ���� ��)
            skillStrategy.Initialize(skillEffect);
            skillStrategy.SkillRange = data.skillRange;

            Debug.Log($"[SkillStrategyFactory] ��ų ���� ���� �Ϸ�: {skillStrategy.GetType().Name}");
            return skillStrategy;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillStrategyFactory] ��ų ���� ���� �� ����: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// ���� ��ų �����͸� �����մϴ�.
    /// </summary>
    private static void SetupBuffData(SkillConfig config, BossData bossData)
    {
        // Ŀ���� ���� ������ ����
        BuffData buffData = new BuffData();

        // ���� Ÿ��, ���ӽð�, ��ġ�� �Ľ� �� ����
        buffData.buffTypes = ParseBuffTypes(config.buffTypes);
        buffData.durations = ParseFloatArray(config.buffDurations);
        buffData.values = ParseFloatArray(config.buffValues);

        // ���� �����Ϳ� ���� ������ ������Ʈ
        bossData.buffData = buffData;

        Debug.Log($"[SkillStrategyFactory] ���� ������ ���� �Ϸ�: ���� Ÿ�� �� {buffData.buffTypes.Length}");
    }

    /// <summary>
    /// ���� Ÿ�� ���ڿ��� BuffType �迭�� ��ȯ�մϴ�.
    /// </summary>
    private static BuffType[] ParseBuffTypes(string buffTypesString)
    {
        if (string.IsNullOrEmpty(buffTypesString))
            return new BuffType[0];

        string[] types = buffTypesString.Split('|');
        BuffType[] result = new BuffType[types.Length];

        for (int i = 0; i < types.Length; i++)
        {
            if (System.Enum.TryParse(types[i], out BuffType buffType))
                result[i] = buffType;
            else
                result[i] = BuffType.None;
        }

        return result;
    }

    /// <summary>
    /// �����ڷ� �������� float �� ���ڿ��� float �迭�� ��ȯ�մϴ�.
    /// </summary>
    private static float[] ParseFloatArray(string floatString)
    {
        if (string.IsNullOrEmpty(floatString))
            return new float[0];

        string[] values = floatString.Split('|');
        float[] result = new float[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            if (float.TryParse(values[i], out float value))
                result[i] = value;
        }

        return result;
    }
}