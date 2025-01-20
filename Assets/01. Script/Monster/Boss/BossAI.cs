using static IMonsterState;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BossAI : CreatureAI
{
    private Dictionary<int, Action> phasePatterns;
    private int currentPhase = 1;
    private float[] phaseThresholds;  // HP 비율 기준으로 페이즈 전환 (예: 0.7f, 0.4f, 0.2f)
    private float currentPhaseThreshold = 1.0f;

    private Dictionary<int, List<AttackPatternData>> phasePatternData;  // 페이즈별 가능한 패턴들
    private AttackPatternData currentPattern;                           // 현재 실행 중인 패턴
    private int currentStepIndex;                                       // 현재 패턴의 스텝 인덱스

    protected override void Start()
    {
        base.Start();
        InitializePhases();
    }

    private void InitializePhases()
    {
        // 구체 클래스로 GetComponent 후 인터페이스로 사용
        BossStatus bossStatusComponent = GetComponent<BossStatus>();
        if (bossStatusComponent == null) return;

        IMonsterClass monster = bossStatusComponent.GetMonsterClass();
        if (monster == null) return;

        ICreatureData data = monster.GetMonsterData();
        if (!(data is BossData bossData)) return;

        phasePatternData = new Dictionary<int, List<AttackPatternData>>();
        foreach (var phaseData in bossData.phaseData)
        {
            int phaseNumber = bossData.phaseData.IndexOf(phaseData) + 1;
            phasePatternData[phaseNumber] = phaseData.availablePatterns;
        }
    }

    protected override void InitializeStates()
    {
        BossStatus bossStatusComponent = GetComponent<BossStatus>();
        IMonsterClass monster = bossStatusComponent.GetMonsterClass();
        ICreatureData data = monster.GetMonsterData();

        InitializeStrategies(data);
        InitializeSkillEffect(data);

        states = new Dictionary<IMonsterState.MonsterStateType, IMonsterState>
       {
           { MonsterStateType.Spawn, new SpawnState(this, spawnStrategy) },
           { MonsterStateType.Idle, new IdleState(this, idleStrategy) },
           { MonsterStateType.Move, new MoveState(this, moveStrategy) },
           { MonsterStateType.Attack, new AttackState(this, attackStrategy) },
           { MonsterStateType.Skill, new SkillState(this, skillStrategy) },
           { MonsterStateType.Hit, new HitState(this, hitStrategy) },
           { MonsterStateType.Groggy, new GroggyState(this, groggyStrategy) },
           { MonsterStateType.Die, new DieState(this, dieStrategy) }
       };

        ChangeState(MonsterStateType.Spawn);
    }

    private void InitializeStrategies(ICreatureData data)
    {
        spawnStrategy = StrategyFactory.CreateSpawnStrategy(data.spawnStrategy);
        moveStrategy = StrategyFactory.CreateMovementStrategy(data.moveStrategy);
        attackStrategy = StrategyFactory.CreateAttackStrategy(data.attackStrategy, data);
        idleStrategy = StrategyFactory.CreatIdleStrategy(data.idleStrategy);
        skillStrategy = StrategyFactory.CreateSkillStrategy(data.skillStrategy, this);
        dieStrategy = StrategyFactory.CreatDieStrategy(data.dieStrategy);
        hitStrategy = StrategyFactory.CreatHitStrategy(data.hitStrategy);
        groggyStrategy = StrategyFactory.CreateGroggyStrategy(data.groggyStrategy, data);

        if (data is BossData bossData)
        {
            attackStrategy = new PatternBasedAttackStrategy();
            ((PatternBasedAttackStrategy)attackStrategy).Initialize(bossData);
        }
    }

    private void InitializeSkillEffect(ICreatureData data)
    {
        ISkillEffect skillEffect = StrategyFactory.CreateSkillEffect(
            data.skillEffectType,
            data,
            this
        );

        if (skillEffect != null)
        {
            skillStrategy.Initialize(skillEffect);
            skillStrategy.SkillRange = data.skillRange;
        }
        else
        {
            Debug.LogError($"Failed to create skill effect for monster: {data.MonsterName}");
        }
    }

    protected override void Update()
    {
        base.Update();
        CheckPhaseTransition();
        ExecuteCurrentPhasePattern();
    }

    private void CheckPhaseTransition()
    {
        IMonsterClass monster = creatureStatus.GetMonsterClass();
        float healthRatio = (float)monster.CurrentHealth / monster.MaxHealth;

        for (int i = 0; i < phaseThresholds.Length; i++)
        {
            if (healthRatio <= phaseThresholds[i] && currentPhaseThreshold > phaseThresholds[i])
            {
                currentPhaseThreshold = phaseThresholds[i];
                TransitionToPhase(i + 2);  // i + 2는 다음 페이즈 번호
                break;
            }
        }
    }

    private void TransitionToPhase(int newPhase)
    {
        currentPhase = newPhase;
        // 페이즈 전환 효과나 특수 동작 실행
        PlayPhaseTransitionEffect();
    }

    private void ExecuteCurrentPhasePattern()
    {
        if (phasePatterns.ContainsKey(currentPhase))
        {
            phasePatterns[currentPhase].Invoke();
        }
    }

    #region Phase Patterns
    private void ExecutePhase1Pattern()
    {
        // 기본 패턴
        // 예: 일반 공격과 기본 스킬 사용
    }

    private void ExecutePhase2Pattern()
    {
        // 2페이즈 패턴
        // 예: 더 공격적인 패턴, 새로운 스킬 추가
    }

    private void ExecutePhase3Pattern()
    {
        // 3페이즈 패턴
        // 예: 광폭화, 연속 공격 등
    }
    #endregion

    private void PlayPhaseTransitionEffect()
    {
        // 페이즈 전환시 이펙트, 사운드 등
    }

    #region Strategy Implementation
    public override IAttackStrategy GetAttackStrategy() => attackStrategy;
    public override ISkillStrategy GetSkillStrategy() => skillStrategy;
    public override ISpawnStrategy GetSpawnStrategy() => spawnStrategy;
    public override IMovementStrategy GetMovementStrategy() => moveStrategy;
    public override IIdleStrategy GetIdleStrategy() => idleStrategy;
    public override IDieStrategy GetDieStrategy() => dieStrategy;
    public override IHitStrategy GetHitStrategy() => hitStrategy;
    public override IGroggyStrategy GetGroggyStrategy() => groggyStrategy;

    public override void SetAttackStrategy(IAttackStrategy newStrategy)
    {
        attackStrategy = newStrategy;
        if (states.ContainsKey(MonsterStateType.Attack))
        {
            states[MonsterStateType.Attack] = new AttackState(this, attackStrategy);
        }
    }

    public override void SetMovementStrategy(IMovementStrategy newStrategy)
    {
        moveStrategy = newStrategy;
        if (states.ContainsKey(MonsterStateType.Move))
        {
            states[MonsterStateType.Move] = new MoveState(this, moveStrategy);
        }
    }

    public override void SetSkillStrategy(ISkillStrategy newStrategy)
    {
        skillStrategy = newStrategy;
        if (states.ContainsKey(MonsterStateType.Skill))
        {
            states[MonsterStateType.Skill] = new SkillState(this, skillStrategy);
        }
    }

    public override void SetIdleStrategy(IIdleStrategy newStrategy)
    {
        idleStrategy = newStrategy;
        if (states.ContainsKey(MonsterStateType.Idle))
        {
            states[MonsterStateType.Idle] = new IdleState(this, idleStrategy);
        }
    }
    #endregion
}