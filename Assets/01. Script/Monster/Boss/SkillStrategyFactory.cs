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

            // Ŀ���� SkillEffect ����
            ISkillEffect skillEffect = null;

            // ������Ÿ�� ����Ʈ�� ��� Ŀ���� ������ Ȯ��
            if (config.effectType == SkillEffectType.Projectile && data is BossData bossData)
            {
                // ���� �������� ��� ��ų�� Ŀ���� ������ Ȯ��
                int bossId = bossData.BossID;
                GameObject customPrefab = BossDataManager.Instance.GetSkillPrefab(bossId, configId);
                GameObject customHitPrefab = BossDataManager.Instance.GetSkillImpactPrefab(bossId, configId);


                if (customPrefab != null)
                {
                    try
                    {
                        // Ŀ���� ���������� ProjectileSkillEffect ����
                        skillEffect = new ProjectileSkillEffect(
                            customPrefab,      // Ŀ���� ������ ���
                            data.projectileSpeed,
                            moveStrategy,
                            impactEffect,
                            customHitPrefab,
                            data.heightFactor
                        );
                        // �߿�: ���⼭ ProjectileSkillEffect�� Initialize ���� ȣ���Ͽ� ������ ��� ����
                        (skillEffect as ProjectileSkillEffect)?.Initialize(
                            owner.GetStatus(),
                            null, // target�� ��ų ���� �� ������
                            config.damageMultiplier,// ������ ��� ����
                            config.speedMultiplier //�߻�ü ���ǵ� ��� ����
                        );
                        Debug.Log($"[SkillStrategyFactory] ���� {bossId}�� ��ų {configId}�� Ŀ���� ������ ���");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[SkillStrategyFactory] ProjectileSkillEffect ���� �� ����: {e.Message}");
                    }
                }
            }
            // Howl ����Ʈ�� ��� Ŀ���� ������ Ȯ��
            else if (config.effectType == SkillEffectType.Howl && data is BossData howlBossData)
            {
                try
                {
                    int bossId = howlBossData.BossID;
                    // �����ϰ� ���ҽ� ��������
                    // SkillStrategyFactory�� ��ų ���� ��� �ڵ忡��
                    GameObject howlEffectPrefab = BossDataManager.Instance.GetHowlEffectPrefab(bossId, configId);
                    GameObject idicatorPrefab = BossDataManager.Instance.GetIndicatorPrefab(bossId, configId);

                    // Ŀ���� ���������� HowlSkillEffect ����
                    float radius = howlBossData.howlRadius > 0 ? howlBossData.howlRadius : howlBossData.skillRange; // �⺻�� ó��
                        float essenceAmount = howlBossData.howlEssenceAmount; // ������ ������
                        float duration = howlBossData.howlDuration; // ���ӽð�
                        float damage = howlBossData.skillDamage * config.damageMultiplier; // ��ų �������� ��� ����

                        skillEffect = new HowlSkillEffect(
                            howlEffectPrefab, // �Ͽ︵ ������
                            howlBossData.areaEffectPrefab, // ����Ʈ ������
                            howlBossData.howlSound, // �����Ҹ�
                            radius,
                            essenceAmount,
                            duration,
                            damage,
                            owner.transform,
                             "������22222222222",
                             idicatorPrefab
                        ); ;
                   
                            

                        // HowlSkillEffect �ʱ�ȭ ȣ��
                        (skillEffect as HowlSkillEffect)?.Initialize(
                            owner.GetStatus(),
                            owner.transform, // target�� ��ų ���� �� ������
                            config.damageMultiplier,
                            config.speedMultiplier > 0 ? config.speedMultiplier : 1.0f // �⺻�� ó��
                        );

                        Debug.Log($"[SkillStrategyFactory] ���� {bossId}�� Howl ��ų {configId}�� Ŀ���� ������ ���");
                    
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SkillStrategyFactory] HowlSkillEffect ���� �� ����: {e.Message}\n{e.StackTrace}");
                }
            }

            // Ŀ���� �������� ���ų� �ٸ� ����Ʈ Ÿ���� ��� �⺻ ����
            if (skillEffect == null)
            {
                try
                {
                    skillEffect = StrategyFactory.CreateSkillEffect(config.effectType, data, owner);

                    // ������Ÿ�� ����Ʈ�� ��� Initialize ȣ��
                    if (config.effectType == SkillEffectType.Projectile)
                    {
                        (skillEffect as ProjectileSkillEffect)?.Initialize(
                            owner.GetStatus(),
                            null, // target�� ��ų ���� �� ������
                            config.damageMultiplier,
                            config.speedMultiplier
                        );
                    }
                    // Howl ����Ʈ�� ��� Initialize ȣ��
                    else if (config.effectType == SkillEffectType.Howl)
                    {
                        (skillEffect as HowlSkillEffect)?.Initialize(
                            owner.GetStatus(),
                            owner.transform, // target�� ��ų ���� �� ������
                            config.damageMultiplier,
                            config.speedMultiplier > 0 ? config.speedMultiplier : 1.0f
                        );
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SkillStrategyFactory] �⺻ ��ų ����Ʈ ���� �� ����: {e.Message}");
                }
            }

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
            if (config.strategyType == SkillStrategyType.Buff && data is BossData bossBuffData)
            {
                SetupBuffData(config, bossBuffData);
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