public static class BossStrategyFactory
{
    // 페이즈 전환 전략
    public static IPhaseTransitionStrategy CreatePhaseTransitionStrategy(
        PhaseTransitionType type,
        BossMonster boss)
    {
        return type switch
        {
            //PhaseTransitionType.AreaAttack => new AreaAttackTransitionStrategy(boss),
            //PhaseTransitionType.TerrainChange => new TerrainChangeTransitionStrategy(boss),
            //PhaseTransitionType.Summon => new SummonTransitionStrategy(boss),
            PhaseTransitionType.Basic => new BossPhaseTransitionStrategy(boss)
        };
    }

    // 기믹 전략
    //public static IGimmickStrategy CreateGimmickStrategy(
    //    GimmickType type,
    //    BossMonster boss)
    //{
    //    return type switch
    //    {
    //        GimmickType.FieldHazard => new FieldHazardGimmickStrategy(boss),
    //        GimmickType.WavePattern => new WavePatternGimmickStrategy(boss),
    //        GimmickType.EnvironmentChange => new EnvironmentGimmickStrategy(boss),
    //        _ => new BasicGimmickStrategy(boss)
    //    };
    //}
}