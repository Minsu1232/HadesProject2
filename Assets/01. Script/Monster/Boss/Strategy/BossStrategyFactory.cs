using UnityEngine;

public static class BossStrategyFactory
{
    // 페이즈 전환 전략
    public static IPhaseTransitionStrategy CreatePhaseTransitionStrategy(
        PhaseTransitionType type,
        BossMonster boss,
        BossAI bossAI,
        BossUIManager bossUIManager)
    {
        return type switch
        {
            //PhaseTransitionType.AreaAttack => new AreaAttackTransitionStrategy(boss),
            //PhaseTransitionType.TerrainChange => new TerrainChangeTransitionStrategy(boss),
            //PhaseTransitionType.Summon => new SummonTransitionStrategy(boss),
            PhaseTransitionType.Basic => new BossPhaseTransitionStrategy(boss, bossAI, bossUIManager)
        };
    }

    // 기믹 전략
    public static IGimmickStrategy CreateGimmickStrategy(
        GimmickType type,
        BossAI boss,
        GimmickData data,
        GameObject prefab,
        BossData soundData,
        ISuccessUI successUI
        )
    {
        return type switch
        {
            GimmickType.FieldHazard => new HazardGimmickStrategy(boss, data, prefab, successUI,soundData.roarSound),
            //GimmickType.WavePattern => new WavePatternGimmickStrategy(boss),
            //GimmickType.EnvironmentChange => new EnvironmentGimmickStrategy(boss),
            //_ => new BasicGimmickStrategy(boss)
        };
    }
    public static BossPattern CreatePatternStrategy(
    AttackPatternData patternData,
    BossAI bossAI,
    MiniGameManager miniGameManager,
    BossData bossData)
    {
        return patternData.patternType switch
        {
            BossPatternType.BasicToJump => new BasicToJumpPattern(
                miniGameManager,
                bossData.shorckEffectPrefab,
                bossData.shockwaveRadius,
                bossData,
                bossAI.animator,
                bossAI,
                patternData
            ),
            BossPatternType.JumpToBasic => new JumpToBasicPattern(
           miniGameManager,
           bossData.shorckEffectPrefab,
           bossData.shockwaveRadius,
           bossData,
           bossAI.animator,
           bossAI,
           patternData
       ),
            _ => throw new System.ArgumentException($"Unknown pattern type: {patternData.patternType}")
        };
    }
}