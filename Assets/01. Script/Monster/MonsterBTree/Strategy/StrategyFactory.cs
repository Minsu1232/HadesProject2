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

    public static IAttackStrategy CreateAttackStrategy(AttackStrategyType type)
    {
        return type switch
        {
            //AttackStrategyType.Melee => new MeleeAttackStrategy(),
            //AttackStrategyType.Ranged => new RangedAttackStrategy(),
            //AttackStrategyType.AoE => new AoEAttackStrategy(),
            //AttackStrategyType.Combo => new ComboAttackStrategy(),
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

    public static ISkillStrategy CreateSkillStrategy(SkillStrategyType type, MonsterAI owner)
    {
        return type switch
        {
            //SkillStrategyType.Buff => new BuffSkillStrategy(),
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
            _ => new StraightMovement(),
        };
    }

    public static ISkillEffect CreateSkillEffect(SkillEffectType effectType, MonsterData data, MonsterAI owner)
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
                return new ProjectileSkillEffect(data.projectilePrefab, data.projectileSpeed, moveStrategy);

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
                    data.buffType,
                    data.buffDuration,
                    data.buffValue  // MonsterData에 추가 필요
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