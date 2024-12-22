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

    public static ISkillStrategy CreateSkillStrategy(SkillStrategyType type)
    {
        return type switch
        {
            //SkillStrategyType.Buff => new BuffSkillStrategy(),
            //SkillStrategyType.Debuff => new DebuffSkillStrategy(),
            //SkillStrategyType.Summon => new SummonSkillStrategy(),
            //SkillStrategyType.AreaControl => new AreaControlSkillStrategy(),
            _ => new BasicSkillStrategy()
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

            _ => new BasicHitStrategy()

        };
    }
}