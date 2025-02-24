using UnityEngine;

public static class StrategyFactory
{
    public static ISpawnStrategy CreateSpawnStrategy(SpawnStrategyType type)
    {
        return type switch
        {
            //SpawnStrategyType.Portal => new PortalSpawnStrategy(),
            //SpawnStrategyType.Summon => new SummonSpawnStrategy(),
            _ => new BasicSpawnStrategy()
        };
    }

    public static IMovementStrategy CreateMovementStrategy(MovementStrategyType type)
    {
        return type switch
        {
            //MovementStrategyType.Aggressive => new AggressiveMovementStrategy(),
            //MovementStrategyType.Defensive => new DefensiveMovementStrategy(),
            //MovementStrategyType.Ranged => new RangedMovementStrategy(),
            //MovementStrategyType.Patrol => new PatrolMovementStrategy(),
            MovementStrategyType.Retreat => new RetreatMovementStrategy(),
            _ => new BasicMovementStrategy()
        };
    }

    public static IAttackStrategy CreateAttackStrategy(AttackStrategyType type, ICreatureData data, CreatureAI creatureAI)
    {
        return type switch
        {
            AttackStrategyType.Jump => new JumpAttackStrategy(data.shorckEffectPrefab,data.shockwaveRadius, creatureAI),
            AttackStrategyType.Combo => new ComboAttackStrategy(),
            AttackStrategyType.Charge => new ChargeAttackStrategy(creatureAI,data,creatureAI.animator),
            _ => new BasicAttackStrategy()
        };;
    }

    public static IIdleStrategy CreatIdleStrategy(IdleStrategyType type)
    {
        return type switch
        {
            //IdleStrategyType.Defensive => new DefensiveIdleStrategy(),
            //IdleStrategyType.Alert => new AlertIdleStrategy(),
            //IdleStrategyType.Passive => new PassiveIdleStrategy(),
            _ => new BasicIdleStrategy()
        };
    }

    public static ISkillStrategy CreateSkillStrategy(SkillStrategyType type, CreatureAI owner)
    {
        return type switch
        {
            SkillStrategyType.Buff => new BuffSkillStrategy(owner),
            //SkillStrategyType.Debuff => new DebuffSkillStrategy(),
            //SkillStrategyType.Summon => new SummonSkillStrategy(),
            //SkillStrategyType.AreaControl => new AreaControlSkillStrategy(),
            SkillStrategyType.MultiShot => new MultiShotSkillStrategy(owner),
            _ => new BasicSkillStrategy(owner)
        };
    }

    public static IDieStrategy CreatDieStrategy(DieStrategyType type)
    {
        return type switch
        {
            //DieStrategyType.Explosion => new ExplosionDieStrategy(),
            //DieStrategyType.Split => new SplitDieStrategy(),
            //DieStrategyType.Resurrection => new ResurrectionDieStrategy(),
            //DieStrategyType.DropItem => new DropItemDieStrategy(),
            _ => new BasicDieStrategy()
        };
    }

    public static IHitStrategy CreatHitStrategy(HitStrategyType type)
    {
        return type switch
        {
            //HitStrategyType.Elite => new EliteHitStrategy(),
            //HitStrategyType.MiniBoss => new MiniBossHitStrategy(),
            //HitStrategyType.Boss => new BossHitStrategy(),
            _ => new BasicHitStrategy()
        };
    }

    public static IProjectileMovement CreateProjectileMovement(ProjectileMovementType type, ICreatureData data)
    {
        return type switch
        {
            ProjectileMovementType.Homing => new HomingMovement(),
            ProjectileMovementType.Parabolic => new ParabolicMovement(),
            ProjectileMovementType.Straight => new StraightMovement(),
            ProjectileMovementType.StraightRotation => new StraightRotationMovement(
                data.projectileRotationAxis,  // MonsterData���� ������ ȸ����
                data.projectileRotationSpeed  // MonsterData���� ������ ȸ�� �ӵ�
            ),
        };
    }

    public static IProjectileImpact CreateProjectileImpact(ProjectileImpactType type, ICreatureData data)
    {
        return type switch
        {
            ProjectileImpactType.Poison => new AreaImpact(
                data.areaEffectPrefab,
                data.areaDuration,
                data.areaRadius
            ),
            _ => null
        };
    }
    public static IGroggyStrategy CreateGroggyStrategy(GroggyStrategyType type, ICreatureData data)
    {
        return type switch
        {
            //GroggyStrategyType.Elite => new EliteGroggyStrategy(data.groggyTime),
            //GroggyStrategyType.Boss => new BossGroggyStrategy(data.groggyTime),
            _ => new BasicGroggyStrategy(data.groggyTime)
        };
    }
    public static ISkillEffect CreateSkillEffect(SkillEffectType effectType, ICreatureData data, CreatureAI owner)
    {
        switch (effectType)
        {
            case SkillEffectType.Projectile:
                if (data.projectilePrefab == null)
                {
                    Debug.LogError($"Projectile prefab is missing for monster: {data.MonsterName}");
                    return null;
                }
                var moveStrategy = CreateProjectileMovement(data.projectileType,data);
                var impactEffect = CreateProjectileImpact(data.projectileImpactType, data);
                return new ProjectileSkillEffect(
                    data.projectilePrefab,
                    data.projectileSpeed,
                    moveStrategy,
                    impactEffect,
                    data.hitEffect
                );

            case SkillEffectType.AreaEffect:
                if (data.areaEffectPrefab == null)
                {
                    Debug.LogError($"Area effect prefab is missing for monster: {data.MonsterName}");
                    return null;
                }
                return new AreaSkillEffect(
                    data.areaEffectPrefab,
                    data.areaRadius,
                    data.skillDamage
                );

            case SkillEffectType.Buff:
                return new BuffSkillEffect(
                   data.buffData.buffTypes,    // ���� ���� Ÿ�� �迭
        data.buffData.durations,    // �� ������ ���ӽð� �迭
        data.buffData.values,        // �� ������ ��ġ�� �迭
        data.buffEffectPrefab,
        owner.transform
                );

            case SkillEffectType.Summon:
                if (data.summonPrefab == null)
                {
                    Debug.LogError($"Summon prefab is missing for monster: {data.MonsterName}");
                    return null;
                }
                return new SummonSkillEffect(
                    data.summonPrefab,
                    data.summonCount,
                    data.summonRadius  // MonsterData�� �߰� �ʿ�
                );

            default:
                Debug.LogError($"Unknown skill effect type: {effectType} for monster: {data.MonsterName}");
                return null;
        }
    }

}

// AttackStrategyFactory Ȯ��
public static class AttackStrategyFactory
{
    private static GameObject shockwaveEffectPrefab; // ����Ʈ ������ ���� ����

    public static void Initialize(GameObject shockwaveEffect)
    {
        shockwaveEffectPrefab = shockwaveEffect;
    }

    public static IAttackStrategy CreateStrategy(AttackStrategyType type)
    {
        return type switch
        {
            //AttackStrategyType.Jump => new JumpAttackStrategy(shockwaveEffectPrefab),
            //AttackStrategyType.Charge => new ChargeAttackStrategy(), // ���� �ʿ�
            //AttackStrategyType.Combo => new ComboAttackStrategy(),   // ���� �ʿ�
            _ => new BasicAttackStrategy()
        };
    }
}