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

    public static IAttackStrategy CreateAttackStrategy(AttackStrategyType type, MonsterData data)
    {
        return type switch
        {
            AttackStrategyType.Jump => new JumpAttackStrategy(data.ShorckEffectPrefab,data.shockwaveRadius),
            _ => new BasicAttackStrategy()
        };
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

    public static IProjectileMovement CreateProjectileMovement(ProjectileMovementType type)
    {
        return type switch
        {
            ProjectileMovementType.Homing => new HomingMovement(),
            ProjectileMovementType.Parabolic => new ParabolicMovement(),
            ProjectileMovementType.Straight => new StraightMovement(),
            _ => null
        };
    }

    public static IProjectileImpact CreateProjectileImpact(ProjectileImpactType type, MonsterData data)
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
    public static IGroggyStrategy CreateGroggyStrategy(GroggyStrategyType type, MonsterData data)
    {
        return type switch
        {
            //GroggyStrategyType.Elite => new EliteGroggyStrategy(data.groggyTime),
            //GroggyStrategyType.Boss => new BossGroggyStrategy(data.groggyTime),
            _ => new BasicGroggyStrategy(data.groggyTime)
        };
    }
    public static ISkillEffect CreateSkillEffect(SkillEffectType effectType, MonsterData data, CreatureAI owner)
    {
        switch (effectType)
        {
            case SkillEffectType.Projectile:
                if (data.projectilePrefab == null)
                {
                    Debug.LogError($"Projectile prefab is missing for monster: {data.monsterName}");
                    return null;
                }
                var moveStrategy = CreateProjectileMovement(data.projectileType);
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
                    Debug.LogError($"Area effect prefab is missing for monster: {data.monsterName}");
                    return null;
                }
                return new AreaSkillEffect(
                    data.areaEffectPrefab,
                    data.areaRadius,
                    data.skillDamage
                );

            case SkillEffectType.Buff:
                return new BuffSkillEffect(
                   data.buffData.buffTypes,    // 여러 버프 타입 배열
        data.buffData.durations,    // 각 버프의 지속시간 배열
        data.buffData.values,        // 각 버프의 수치값 배열
        data.buffEffectPrefab
                );

            case SkillEffectType.Summon:
                if (data.summonPrefab == null)
                {
                    Debug.LogError($"Summon prefab is missing for monster: {data.monsterName}");
                    return null;
                }
                return new SummonSkillEffect(
                    data.summonPrefab,
                    data.summonCount,
                    data.summonRadius  // MonsterData에 추가 필요
                );

            default:
                Debug.LogError($"Unknown skill effect type: {effectType} for monster: {data.monsterName}");
                return null;
        }
    }

}

// AttackStrategyFactory 확장
public static class AttackStrategyFactory
{
    private static GameObject shockwaveEffectPrefab; // 이펙트 프리팹 참조 저장

    public static void Initialize(GameObject shockwaveEffect)
    {
        shockwaveEffectPrefab = shockwaveEffect;
    }

    public static IAttackStrategy CreateStrategy(AttackStrategyType type)
    {
        return type switch
        {
            //AttackStrategyType.Jump => new JumpAttackStrategy(shockwaveEffectPrefab),
            //AttackStrategyType.Charge => new ChargeAttackStrategy(), // 구현 필요
            //AttackStrategyType.Combo => new ComboAttackStrategy(),   // 구현 필요
            _ => new BasicAttackStrategy()
        };
    }
}