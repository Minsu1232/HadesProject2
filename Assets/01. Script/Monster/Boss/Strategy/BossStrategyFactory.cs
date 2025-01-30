public static class BossStrategyFactory
{
    // ������ ��ȯ ����
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

    // ��� ����
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