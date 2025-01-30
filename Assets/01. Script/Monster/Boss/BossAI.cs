using static IMonsterState;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BossAI : CreatureAI
{
    private Dictionary<int, Action> phasePatterns;
    private int currentPhase = 1;
    private float[] phaseThresholds;  // HP ���� �������� ������ ��ȯ (��: 0.7f, 0.4f, 0.2f)
    private float currentPhaseThreshold = 1.0f;

    private Dictionary<int, List<AttackPatternData>> phasePatternData;  // ����� ������ ���ϵ�
    private AttackPatternData currentPattern;                           // ���� ���� ���� ����
    private int currentStepIndex;                                       // ���� ������ ���� �ε���

    private MiniGameManager miniGameManager;
    private BossUIManager bossUIManager;
   
    private BossMonster bossMonster;
   

    private IPhaseTransitionStrategy currentPhaseStrategy;
    protected override void Start()
    {
        miniGameManager = FindObjectOfType<MiniGameManager>();
        bossUIManager = GetComponent<BossUIManager>();
        base.Start();

        InitializePhases();
        InitializeBehaviorTree();
        
        
    }

    private void InitializePhases()
    {
        // ��ü Ŭ������ GetComponent �� �������̽��� ���
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
        IMonsterClass monsterClass = creatureStatus.GetMonsterClass();
        ICreatureData data = monsterClass.GetMonsterData();
        

        InitializeStrategies(data);
        InitializeSkillEffect(data);

        // ���� ���� ���� ��������
        bossMonster = creatureStatus.GetMonsterClass() as BossMonster;
        BossData bossData = bossMonster.GetBossData();
        currentPhaseStrategy = BossStrategyFactory.CreatePhaseTransitionStrategy(bossMonster.CurrentPhaseData.phaseTransitionType, bossMonster);
        //// �������� Ư�� ���� �߰�
        //states[MonsterStateType.PhaseTransition] = new PhaseTransitionState(
        //    this,
        //    BossStrategyFactory.CreatePhaseTransitionStrategy(
        //        bossMonster.CurrentPhaseData.phaseTransitionType,
        //        bossMonster
        //    )
        //);

        //states[MonsterStateType.Gimmick] = new GimmickState(
        //    this,
        //    BossStrategyFactory.CreateGimmickStrategy(
        //        bossMonster.CurrentPhaseData.gimmickType,
        //        bossMonster
        //    )
        //);
        states = new Dictionary<IMonsterState.MonsterStateType, IMonsterState>
       {
           { MonsterStateType.Spawn, new SpawnState(this, spawnStrategy) },
           { MonsterStateType.Idle, new IdleState(this, idleStrategy) },
           { MonsterStateType.Move, new MoveState(this, moveStrategy) },
           { MonsterStateType.Attack, new AttackState(this, attackStrategy) },
           { MonsterStateType.Skill, new SkillState(this, skillStrategy) },
           { MonsterStateType.Hit, new HitState(this, hitStrategy) },
           { MonsterStateType.Groggy, new GroggyState(this, groggyStrategy,bossUIManager) },
           { MonsterStateType.Die, new DieState(this, dieStrategy) },
            {MonsterStateType.PhaseTransition, new PhaseTransitionState(this,currentPhaseStrategy,bossMonster) }
       };

        ChangeState(MonsterStateType.Spawn);
    }
    public void UpdatePhaseStrategies()
    {
        if (bossMonster == null) return;

        // ���� ����� �´� ���� ����
        var currentPhase = bossMonster.CurrentPhaseData;

        if (states[MonsterStateType.PhaseTransition] is PhaseTransitionState phaseState)
        {
            var newStrategy = BossStrategyFactory.CreatePhaseTransitionStrategy(
                currentPhase.phaseTransitionType,
                bossMonster
            );
            phaseState.UpdateStrategy(newStrategy);
        }
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
           if(miniGameManager != null)
            {
                var patternStrategy = new PatternBasedAttackStrategy();
                patternStrategy.Initialize(bossData, miniGameManager,this);
                attackStrategy = patternStrategy;
            }
           
            
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
        
        behaviorTree?.Execute();

       

    }
   
    public void InitializeBehaviorTree()
    {
        behaviorTree = new Selector(this,
         //������ ��ȯ üũ �� ����
        new BTSequence(this,
            new CheckPhaseTransitionNode(this),
            new ExecutePhaseTransitionNode(this)
        ),

        // ���� ����
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
    private void TransitionToPhase(int newPhase)
    {
        currentPhase = newPhase;
        // ������ ��ȯ ȿ���� Ư�� ���� ����
        PlayPhaseTransitionEffect();
    }

  

    #region Phase Patterns
    private void ExecutePhase1Pattern()
    {
        // �⺻ ����
        // ��: �Ϲ� ���ݰ� �⺻ ��ų ���
    }

    private void ExecutePhase2Pattern()
    {
        // 2������ ����
        // ��: �� �������� ����, ���ο� ��ų �߰�
    }

    private void ExecutePhase3Pattern()
    {
        // 3������ ����
        // ��: ����ȭ, ���� ���� ��
    }
    #endregion

    private void PlayPhaseTransitionEffect()
    {
        // ������ ��ȯ�� ����Ʈ, ���� ��
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

    public override IPhaseTransitionStrategy GetPhaseTransitionStrategy() => currentPhaseStrategy;


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