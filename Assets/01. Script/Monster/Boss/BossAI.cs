using static IMonsterState;
using System.Collections.Generic;
using System;
using UnityEngine;
using static BossMultiAttackStrategy;

public class BossAI : CreatureAI
{
    private Dictionary<int, Action> phasePatterns;
    private int currentPhase = 1;
    private float[] phaseThresholds;  // HP 비율 기준으로 페이즈 전환 (예: 0.7f, 0.4f, 0.2f)
    private float currentPhaseThreshold = 1.0f;

    private Dictionary<int, List<AttackPatternData>> phasePatternData;  // 페이즈별 가능한 패턴들
    private AttackPatternData currentPattern;                           // 현재 실행 중인 패턴
    private int currentStepIndex;                                       // 현재 패턴의 스텝 인덱스

    private MiniGameManager miniGameManager;
    private BossUIManager bossUIManager;
   
    private BossMonster bossMonster;
   

    private IPhaseTransitionStrategy currentPhaseStrategy;
    private IGimmickStrategy gimmickStrategy;
    protected override void Start()
    {
        miniGameManager = FindObjectOfType<MiniGameManager>();
        bossUIManager = GetComponent<BossUIManager>();
        base.Start();

        //InitializePhases();
        InitializeBehaviorTree();
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
            int phaseNumber = bossMonster.CurrentPhase;
            phasePatternData[phaseNumber] = phaseData.availablePatterns;
        }
    }

    protected override void InitializeStates()
    {
        IMonsterClass monsterClass = creatureStatus.GetMonsterClass();
        ICreatureData data = monsterClass.GetMonsterData();

        // 1. states 딕셔너리 초기화
        states = new Dictionary<IMonsterState.MonsterStateType, IMonsterState>();

        // 2. 보스 몬스터 참조 초기화
        bossMonster = creatureStatus.GetMonsterClass() as BossMonster;
        BossData bossData = bossMonster.GetBossData();
      
        states[MonsterStateType.Spawn] = new SpawnState(this, spawnStrategy ?? new BasicSpawnStrategy());
        states[MonsterStateType.Idle] = new IdleState(this, idleStrategy ?? new BasicIdleStrategy());
        states[MonsterStateType.Move] = new MoveState(this, moveStrategy ?? new BasicMovementStrategy());
        states[MonsterStateType.Attack] = new AttackState(this, attackStrategy ?? new BasicAttackStrategy());
        states[MonsterStateType.Skill] = new SkillState(this, skillStrategy ?? new BasicSkillStrategy(this));
        states[MonsterStateType.Hit] = new HitState(this, hitStrategy ?? new BasicHitStrategy());
        states[MonsterStateType.Groggy] = new GroggyState(this, groggyStrategy ?? new BasicGroggyStrategy(3f), bossUIManager);
        states[MonsterStateType.Die] = new DieState(this, dieStrategy ?? new BasicDieStrategy());
        states[MonsterStateType.PhaseTransition] = new PhaseTransitionState(this, currentPhaseStrategy ?? new BossPhaseTransitionStrategy(bossMonster, this), bossMonster);
        states[MonsterStateType.Gimmick] = new GimmickState(this, gimmickStrategy ?? new HazardGimmickStrategy());
        // 3. 모든 전략 초기화
        InitializeStrategies(data);

        // 4. states에 전략 할당
        states[MonsterStateType.Spawn] = new SpawnState(this, spawnStrategy);
        states[MonsterStateType.Idle] = new IdleState(this, idleStrategy);
        states[MonsterStateType.Move] = new MoveState(this, moveStrategy);
        states[MonsterStateType.Attack] = new AttackState(this, attackStrategy);
        states[MonsterStateType.Skill] = new SkillState(this, skillStrategy);
        states[MonsterStateType.Hit] = new HitState(this, hitStrategy);
        states[MonsterStateType.Groggy] = new GroggyState(this, groggyStrategy, bossUIManager);
        states[MonsterStateType.Die] = new DieState(this, dieStrategy);
        states[MonsterStateType.PhaseTransition] = new PhaseTransitionState(this, currentPhaseStrategy, bossMonster);
        states[MonsterStateType.Gimmick] = new GimmickState(this, gimmickStrategy);

        // 5. 초기 상태 설정
        ChangeState(MonsterStateType.Spawn);
    }
    public void UpdatePhaseStrategies()
    {
        if (bossMonster == null) return;

        // 현재 페이즈에 맞는 전략 생성
        var currentPhase = bossMonster.CurrentPhaseData;

        if (states[MonsterStateType.PhaseTransition] is PhaseTransitionState phaseState)
        {
            var newStrategy = BossStrategyFactory.CreatePhaseTransitionStrategy(
                currentPhase.phaseTransitionType,
                bossMonster,
                this
            );
            phaseState.UpdateStrategy(newStrategy);
        }
    }
       
    
    private void InitializeStrategies(ICreatureData data)
    {
        // 1. 기본 전략들 초기화
        spawnStrategy = StrategyFactory.CreateSpawnStrategy(data.spawnStrategy);
        moveStrategy = StrategyFactory.CreateMovementStrategy(data.moveStrategy);
        idleStrategy = StrategyFactory.CreatIdleStrategy(data.idleStrategy);
        skillStrategy = StrategyFactory.CreateSkillStrategy(data.skillStrategy, this);
        dieStrategy = StrategyFactory.CreatDieStrategy(data.dieStrategy);
        hitStrategy = StrategyFactory.CreatHitStrategy(data.hitStrategy);
        groggyStrategy = StrategyFactory.CreateGroggyStrategy(data.groggyStrategy, data);

        // 2. 보스 특수 전략들 초기화
        if (data is BossData bossData)
        {
            // 페이즈 및 기믹 전략 초기화
            currentPhaseStrategy = BossStrategyFactory.CreatePhaseTransitionStrategy(
                bossMonster.CurrentPhaseData.phaseTransitionType,
                bossMonster,
                this
            );

            gimmickStrategy = BossStrategyFactory.CreateGimmickStrategy(
                bossMonster.CurrentPhaseGimmickData.type,
                this,
                bossMonster.CurrentPhaseGimmickData,
                bossMonster.CurrentPhaseGimmickData.hazardPrefab
            );

            // 공격 전략 설정
            if (miniGameManager != null)
            {
                SetupPhaseAttackStrategies(bossData.phaseData[currentPhase], bossData);
            }
            else
            {
                Debug.LogError("MiniGameManager is null!");
            }
        }

        // 3. 스킬 이펙트 초기화
        InitializeSkillEffect(data);

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
   public void SetupPhaseAttackStrategies(PhaseData phaseData, BossData bossData)
    {
    
        var strategies = new List<IAttackStrategy>();
        var weights = new List<float>();

        // 1. 패턴 기반 공격 추가
        if (miniGameManager != null)
        {
            var patternStrategy = new PatternBasedAttackStrategy();
            patternStrategy.Initialize(bossData, miniGameManager, this, bossMonster);
            strategies.Add(patternStrategy);
            weights.Add(phaseData.patternStrategyWeight);
            patternStrategy.OnPhaseChanged(bossMonster.CurrentPhase);
            Debug.Log(patternStrategy.ToString());
        }
        if (phaseData.phaseName == "Enraged")  // 원하는 조건으로 변경
        {
            Debug.Break();
        }
        // 2. 현재 페이즈의 추가 전략들 205 이부분 문제
        foreach (var strategyData in phaseData.phaseAttackStrategies)
        {
            Debug.Log(strategyData.type);
            var strategy = StrategyFactory.CreateAttackStrategy(strategyData.type, bossData);
            if (strategy != null)
            {
                strategies.Add(strategy);
                weights.Add(strategyData.weight);
                Debug.Log(strategy.ToString());
            }
        }

        Debug.Log("여기부터 시작");
        Debug.Log(strategies[0].ToString());
        Debug.Log(phaseData.phaseName);

        // 3. MultiAttackStrategy 설정
        if (strategies.Count > 0)
        {   
            Debug.Log(strategies.ToString());

            var multiStrategy = new BossMultiAttackStrategy(strategies, weights);
           
            SetAttackStrategy(multiStrategy);  // 이걸로 변경

        }

        InitializePhases();
    }

    protected override void Update()
    {
        base.Update();       
        
        behaviorTree?.Execute();
        if(bossMonster != null)
        {
            Debug.Log($"뭐야 {bossMonster.CurrentPhase}"); 
        }
     
        Debug.Log(currentState);

    }
   
    public void InitializeBehaviorTree()
    {
        behaviorTree = new Selector(this,
         //페이즈 전환 체크 및 실행
        new BTSequence(this,
            new CheckPhaseTransitionNode(this),
            new ExecutePhaseTransitionNode(this)
        ),

        new BTSequence(this,
        new CheckGimmickNode(this),
        new ExecuteGimmickNode(this)
        ),
        // 기존 로직
        new BTSequence(this,
            new CheckHealthCondition(this),
            new ChangeStateAction(this, MonsterStateType.Move, MovementStrategyType.Retreat)
        ),
        new BTSequence(this,
            new CheckPlayerInAttackRange(this),
            new CheckPatternDistance(this),
            new CombatDecisionNode(this)
        ),
        new BTSequence(this,
            new CheckPlayerInRange(this),
            new ChangeStateAction(this, MonsterStateType.Move, MovementStrategyType.Basic)
        )
    );
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



    #region Strategy Implementation
    public override IAttackStrategy GetAttackStrategy() => attackStrategy;
    public override ISkillStrategy GetSkillStrategy() => skillStrategy;
    public override ISpawnStrategy GetSpawnStrategy() => spawnStrategy;
    public override IMovementStrategy GetMovementStrategy() => moveStrategy;
    public override IIdleStrategy GetIdleStrategy() => idleStrategy;
    public override IDieStrategy GetDieStrategy() => dieStrategy;
    public override IHitStrategy GetHitStrategy() => hitStrategy;
    public override IGroggyStrategy GetGroggyStrategy() => groggyStrategy;
    public override IGimmickStrategy GetGimmickStrategy() => gimmickStrategy;
    public override IPhaseTransitionStrategy GetPhaseTransitionStrategy() => currentPhaseStrategy;

    public BossMonster GetBossMonster()
    {
        return bossMonster;
    }
    public override void SetAttackStrategy(IAttackStrategy newStrategy)
    {
        Debug.Log(newStrategy);
        attackStrategy = newStrategy;
        Debug.Log(newStrategy);
        if(states == null)
        {
            Debug.Log("비었다");
        }
        if (states.ContainsKey(MonsterStateType.Attack))
        {
            Debug.Log("새전략으로 바꿈");
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