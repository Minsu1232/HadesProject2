using UnityEngine;

public static class BossStrategyFactory
{
    // ������ ��ȯ ����
    public static IPhaseTransitionStrategy CreatePhaseTransitionStrategy(
        PhaseTransitionType type,
        BossMonster boss,
        BossAI bossAI)
    {
        return type switch
        {
            //PhaseTransitionType.AreaAttack => new AreaAttackTransitionStrategy(boss),
            //PhaseTransitionType.TerrainChange => new TerrainChangeTransitionStrategy(boss),
            //PhaseTransitionType.Summon => new SummonTransitionStrategy(boss),
            PhaseTransitionType.Basic => new BossPhaseTransitionStrategy(boss, bossAI)
        };
    }

    // ��� ����
    public static IGimmickStrategy CreateGimmickStrategy(
        GimmickType type,
        BossAI boss,
        GimmickData data,
        GameObject prefab
        )
    {
        return type switch
        {
            GimmickType.FieldHazard => new HazardGimmickStrategy(boss, data, prefab),
            //GimmickType.WavePattern => new WavePatternGimmickStrategy(boss),
            //GimmickType.EnvironmentChange => new EnvironmentGimmickStrategy(boss),
            //_ => new BasicGimmickStrategy(boss)
        };
    }
}