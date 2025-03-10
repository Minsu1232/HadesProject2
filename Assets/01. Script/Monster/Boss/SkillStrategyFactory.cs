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

            // ���� ������ ��������
            BossData bossData = data as BossData;
            int bossId = bossData?.BossID ?? 0;

            // ����Ʈ Ÿ�Ժ� ó��
            switch (config.effectType)
            {
                case SkillEffectType.Projectile:
                    if (bossData != null)
                    {
                        skillEffect = CreateProjectileSkillEffect(configId, bossId, bossData, owner,
                                                                 moveStrategy, impactEffect, config);
                    }
                    break;

                case SkillEffectType.CircularProjectile:
                    if (bossData != null)
                    {
                        skillEffect = CreateCircularProjectileSkillEffect(configId, bossId, bossData, owner,
                                                                        moveStrategy, impactEffect, config);
                    }
                    break;

                case SkillEffectType.Howl:
                    if (bossData != null)
                    {
                        skillEffect = CreateHowlSkillEffect(configId, bossId, bossData, owner,
                                                          moveStrategy, impactEffect, config);
                    }
                    break;

                default:
                    // �� �� ����Ʈ Ÿ���� �⺻ ���� ���� ���
                    skillEffect = StrategyFactory.CreateSkillEffect(config.effectType, data, owner);
                    break;
            }

            // skillEffect�� null�� ��� �⺻ ���� �õ�
            if (skillEffect == null)
            {
                try
                {
                    skillEffect = StrategyFactory.CreateSkillEffect(config.effectType, data, owner);

                    // Ÿ�Ժ� �߰� �ʱ�ȭ
                    InitializeSkillEffect(skillEffect, config, owner, data);
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
            if (config.strategyType == SkillStrategyType.Buff && bossData != null)
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
    /// ��ų ����Ʈ �ʱ�ȭ
    /// </summary>
    private static void InitializeSkillEffect(ISkillEffect skillEffect, SkillConfig config, CreatureAI owner, ICreatureData data)
    {
        // ������Ÿ�� ����Ʈ�� ��� Initialize ȣ��
        if (config.effectType == SkillEffectType.Projectile && skillEffect is ProjectileSkillEffect projectileEffect)
        {
            projectileEffect.Initialize(
                owner.GetStatus(),
                null, // target�� ��ų ���� �� ������
                config.damageMultiplier,
                config.speedMultiplier
            );
        }
        // CircularProjectile ����Ʈ�� ��� Initialize ȣ��
        else if (config.effectType == SkillEffectType.CircularProjectile && skillEffect is CircularProjectileSkillEffect circularEffect)
        {
            circularEffect.Initialize(
                owner.GetStatus(),
                null,
                config.damageMultiplier,
                config.speedMultiplier
            );
        }
        // Howl ����Ʈ�� ��� Initialize ȣ��
        else if (config.effectType == SkillEffectType.Howl && skillEffect is HowlSkillEffect howlEffect)
        {
            howlEffect.Initialize(
                owner.GetStatus(),
                owner.transform,
                config.damageMultiplier,
                config.speedMultiplier > 0 ? config.speedMultiplier : 1.0f
            );
        }
    }

    /// <summary>
    /// CircularProjectileSkillEffect ����
    /// </summary>
    private static ISkillEffect CreateCircularProjectileSkillEffect(int configId, int bossId, BossData bossData,
                                                    CreatureAI owner, IProjectileMovement moveStrategy,
                                                    IProjectileImpact impactEffect, SkillConfig config)
    {
        try
        {
            // �ʿ��� ������ ��������
            GameObject customPrefab = BossDataManager.Instance.GetSkillPrefab(bossId, configId);
            GameObject customHitPrefab = BossDataManager.Instance.GetSkillImpactPrefab(bossId, configId);

            if (customPrefab == null)
            {
                Debug.LogWarning($"[SkillStrategyFactory] ���� {bossId}�� ��ų {configId}�� �������� �����ϴ�.");
                return null;
            }

            // ���� ���� ����Ʈ Ÿ�� ����
            DelayedExplosionImpact delayedImpact = new DelayedExplosionImpact(
                2f, // ���� ���� �ݰ�
                1.75f, // ���� ���� ���
                bossData.skillDuration, // ���� ���� �ð�
                true, // �⺻������ ����(������) ����
                customHitPrefab
            );

            // CircularProjectileSkillEffect ����
            CircularProjectileSkillEffect circularEffect = new CircularProjectileSkillEffect(
                customPrefab,
                bossData.circleIndicatorPrefab,
                bossData.projectileSpeed,
                moveStrategy,
                delayedImpact, // ���� ���� ����Ʈ ���
                customHitPrefab,
                bossData.heightFactor
            );

            // �߰� �Ķ���� ����
            circularEffect.SetCircleParameters(
                5, // �⺻ �߻�ü ����
                bossData.areaRadius, // ���� ������
                2f, // ���� ���� �ݰ�
                2f, // ���� ���� ���
                bossData.howlDuration,
                bossData.EssenceAmount*1.5f// ���� ���� �ð�
            );

            // Initialize ȣ���Ͽ� ������ ��� ����
            circularEffect.Initialize(
                owner.GetStatus(),
                null, // target�� ��ų ���� �� ������
                config.damageMultiplier, // ������ ��� ����
                config.speedMultiplier // �߻�ü ���ǵ� ��� ����
            );

            Debug.Log($"[SkillStrategyFactory] ���� {bossId}�� CircularProjectile ��ų {configId} ���� �Ϸ�");
            return circularEffect;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillStrategyFactory] CircularProjectileSkillEffect ���� ����: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// ProjectileSkillEffect ����
    /// </summary>
    private static ISkillEffect CreateProjectileSkillEffect(int configId, int bossId, BossData bossData,
                                                CreatureAI owner, IProjectileMovement moveStrategy,
                                                IProjectileImpact impactEffect, SkillConfig config)
    {
        try
        {
            // �ʿ��� ������ ��������
            GameObject customPrefab = BossDataManager.Instance.GetSkillPrefab(bossId, configId);
            GameObject customHitPrefab = BossDataManager.Instance.GetSkillImpactPrefab(bossId, configId);

            if (customPrefab == null)
            {
                Debug.LogWarning($"[SkillStrategyFactory] ���� {bossId}�� ��ų {configId}�� �������� �����ϴ�.");
                return null;
            }

            // ProjectileSkillEffect ����
            ProjectileSkillEffect projectileEffect = new ProjectileSkillEffect(
                customPrefab,
                bossData.projectileSpeed,
                moveStrategy,
                impactEffect,
                customHitPrefab,
                bossData.heightFactor
            );

            // Initialize ȣ���Ͽ� ������ ��� ����
            projectileEffect.Initialize(
                owner.GetStatus(),
                null, // target�� ��ų ���� �� ������
                config.damageMultiplier, // ������ ��� ����
                config.speedMultiplier // �߻�ü ���ǵ� ��� ����
            );

            Debug.Log($"[SkillStrategyFactory] ���� {bossId}�� Projectile ��ų {configId} ���� �Ϸ�");
            return projectileEffect;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillStrategyFactory] ProjectileSkillEffect ���� ����: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// HowlSkillEffect ����
    /// </summary>
    private static ISkillEffect CreateHowlSkillEffect(int configId, int bossId, BossData bossData,
                                         CreatureAI owner, IProjectileMovement moveStrategy,
                                         IProjectileImpact impactEffect, SkillConfig config)
    {
        try
        {
            // �ʿ��� ������ ��������
            GameObject howlEffectPrefab = BossDataManager.Instance.GetHowlEffectPrefab(bossId, configId);
            GameObject indicatorPrefab = BossDataManager.Instance.GetIndicatorPrefab(bossId, configId);
            GameObject areaExplosionPrefab = BossDataManager.Instance.GetSkillAreaPrefab(bossId, configId); 
            if (howlEffectPrefab == null)
            {
                Debug.LogWarning($"[SkillStrategyFactory] ���� {bossId}�� ��ų {configId}�� �Ͽ� ����Ʈ �������� �����ϴ�.");
                return null;
            }

            // �ʿ��� �� ���
            float radius = bossData.howlRadius > 0 ? bossData.howlRadius : bossData.skillRange; // �⺻�� ó��
            float essenceAmount = bossData.EssenceAmount; // ������ ������
            float duration = bossData.howlDuration; // ���ӽð�
            float damage = bossData.skillDamage * config.damageMultiplier; // ��ų �������� ��� ����

            // HowlSkillEffect ����
            HowlSkillEffect howlEffect = new HowlSkillEffect(
                howlEffectPrefab, // �Ͽ︵ ������
                areaExplosionPrefab, // ����Ʈ ������
                bossData.howlSound, // �����Ҹ�
                radius,
                essenceAmount,
                duration,
                damage,
                owner.transform,
                "������22222222222",
                indicatorPrefab
            );

            // Initialize ȣ��
            howlEffect.Initialize(
                owner.GetStatus(),
                owner.transform,
                config.damageMultiplier,
                config.speedMultiplier > 0 ? config.speedMultiplier : 1.0f
            );

            Debug.Log($"[SkillStrategyFactory] ���� {bossId}�� Howl ��ų {configId} ���� �Ϸ�");
            return howlEffect;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillStrategyFactory] HowlSkillEffect ���� ����: {e.Message}\n{e.StackTrace}");
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