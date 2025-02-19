using static IMonsterState;
using System.Collections.Generic;
using System;
using UnityEngine;
using static BossMultiAttackStrategy;

public class BossAI : CreatureAI
{  // ���� ������ BossMultiAttackStrategy �ν��Ͻ��� ����
    private BossMultiAttackStrategy globalMultiAttackStrategy;


    private Dictionary<int, Action> phasePatterns;
    // ���� ���� �̺�Ʈ �߰�
    public event Action<BossPattern> OnPatternChanged;
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
    private IGimmickStrategy gimmickStrategy;
    private ISuccessUI successUI;
    private BossPattern bossPatternStartaegy;
    protected override void Start()
    {
        miniGameManager = FindObjectOfType<MiniGameManager>();
        bossUIManager = GetComponent<BossUIManager>();
        globalMultiAttackStrategy = new BossMultiAttackStrategy();
        successUI = GetComponentInChildren<SuccessUI>();
        base.Start();

        //InitializePhases();
        InitializeBehaviorTree();
        //InitializePhases();


    }

    //private void InitializePhases()
    //{
    //    // ��ü Ŭ������ GetComponent �� �������̽��� ���
    //    BossStatus bossStatusComponent = GetComponent<BossStatus>();
    //    if (bossStatusComponent == null) return;

    //    IMonsterClass monster = bossStatusComponent.GetMonsterClass();
    //    if (monster == null) return;

    //    ICreatureData data = monster.GetMonsterData();
    //    if (!(data is BossData bossData)) return;

    //    phasePatternData = new Dictionary<int, List<AttackPatternData>>();
    //    foreach (var phaseData in bossData.phaseData)
    //    {
    //        int phaseNumber = bossMonster.CurrentPhase;
    //        phasePatternData[phaseNumber] = phaseData.availablePatterns;
    //    }
    //}

    protected override void InitializeStates()
    {
        IMonsterClass monsterClass = creatureStatus.GetMonsterClass();
        ICreatureData data = monsterClass.GetMonsterData();

        // 1. states ��ųʸ� �ʱ�ȭ
        states = new Dictionary<IMonsterState.MonsterStateType, IMonsterState>();

        // 2. ���� ���� ���� �ʱ�ȭ
        bossMonster = creatureStatus.GetMonsterClass() as BossMonster;
        BossData bossData = bossMonster.GetBossData();

        states[MonsterStateType.Spawn] = new SpawnState(this, spawnStrategy ?? new BasicSpawnStrategy());
        states[MonsterStateType.Idle] = new IdleState(this, idleStrategy ?? new BasicIdleStrategy());
        states[MonsterStateType.Move] = new MoveState(this, moveStrategy ?? new BasicMovementStrategy());
        // ���� ������ �������� �����ϴ� globalMultiAttackStrategy�� ����մϴ�.
        states[MonsterStateType.Attack] = new AttackState(this, globalMultiAttackStrategy);
        states[MonsterStateType.Skill] = new SkillState(this, skillStrategy ?? new BasicSkillStrategy(this));
        states[MonsterStateType.Hit] = new HitState(this, hitStrategy ?? new BasicHitStrategy());
        states[MonsterStateType.Groggy] = new GroggyState(this, groggyStrategy ?? new BasicGroggyStrategy(3f), bossUIManager);
        states[MonsterStateType.Die] = new DieState(this, dieStrategy ?? new BasicDieStrategy());
        //states[MonsterStateType.PhaseTransition] = new PhaseTransitionState(this, currentPhaseStrategy,bossMonster ?? new BossPhaseTransitionStrategy(bossMonster, this,bossUIManager));
        states[MonsterStateType.Gimmick] = new GimmickState(this, gimmickStrategy ?? new HazardGimmickStrategy(),bossUIManager);
        // 3. ��� ���� �ʱ�ȭ
        InitializeStrategies(data);

        // 4. states�� ���� �Ҵ�
        states[MonsterStateType.Spawn] = new SpawnState(this, spawnStrategy);
        states[MonsterStateType.Idle] = new IdleState(this, idleStrategy);
        states[MonsterStateType.Move] = new MoveState(this, moveStrategy);
        // ���� ������ �������� �����ϴ� globalMultiAttackStrategy�� ����մϴ�.
        states[MonsterStateType.Attack] = new AttackState(this, globalMultiAttackStrategy);
        states[MonsterStateType.Skill] = new SkillState(this, skillStrategy);
        states[MonsterStateType.Hit] = new HitState(this, hitStrategy);
        states[MonsterStateType.Groggy] = new GroggyState(this, groggyStrategy, bossUIManager);
        states[MonsterStateType.Die] = new DieState(this, dieStrategy);
        states[MonsterStateType.PhaseTransition] = new PhaseTransitionState(this, currentPhaseStrategy, bossMonster);
        states[MonsterStateType.Gimmick] = new GimmickState(this, gimmickStrategy, bossUIManager);
        if (bossMonster.CurrentPhaseData.availablePatterns.Count > 0)
        {
            bossPatternStartaegy = BossStrategyFactory.CreatePatternStrategy(
                bossMonster.CurrentPhaseData.availablePatterns[0],
                this,
                miniGameManager,
                bossData
            );
            states[MonsterStateType.Pattern] = new PatternState(this, bossPatternStartaegy);
        }
        // 5. �ʱ� ���� ����
        ChangeState(MonsterStateType.Spawn);
    }
    public void UpdatePhaseStrategies()
    {
        if (bossMonster == null) return;

        var currentPhase = bossMonster.CurrentPhaseData;
      

        // ���� BossMultiAttackStrategy �ν��Ͻ��� ���� ����(���� ����Ʈ, Ÿ�̸� ��)�� ������ �ʱ�ȭ�մϴ�.
        globalMultiAttackStrategy.ResetAll();

        // ���� �������� ���� ���� �����͸� globalMultiAttackStrategy�� �߰��մϴ�.
        foreach (var strategyData in currentPhase.phaseAttackStrategies)
        {
            var strategy = StrategyFactory.CreateAttackStrategy(strategyData.type, bossMonster.GetBossData(),this);
            if (strategy != null)
            {
                globalMultiAttackStrategy.AddStrategy(strategy, strategyData.weight);
            }
        }

        // ���� ������Ʈ: ���� ����� �ش��ϴ� ������ �ִٸ� ���� ������ �����Ͽ� ���¿� �Ҵ��մϴ�.
        if (currentPhase.availablePatterns.Count > 0)
        {
            var phasePattern = currentPhase.availablePatterns.Find(p => p.phaseNumber == bossMonster.CurrentPhase);

            if (phasePattern != null)
            {
                bossPatternStartaegy.CleanAll();
                bossPatternStartaegy = null;
              
                
                bossPatternStartaegy = BossStrategyFactory.CreatePatternStrategy(
                    phasePattern,
                    this,
                    miniGameManager,
                    bossMonster.GetBossData()
                );
                Debug.Log($"New Pattern Type: {bossPatternStartaegy.GetType().Name}");

                //// PatternState ������Ʈ ���� ����
                //Debug.Log($"Current Pattern State Type before update: {states[MonsterStateType.Pattern].GetType().Name}");
                states[MonsterStateType.Pattern] = new PatternState(this, bossPatternStartaegy);
                OnPatternChanged?.Invoke(bossPatternStartaegy);
            }
            else
            {
                Debug.LogWarning($"No pattern found for phase {bossMonster.CurrentPhase}");
            }
        }
    }


    private void InitializeStrategies(ICreatureData data)
    {
        // 1. �⺻ ������ �ʱ�ȭ
        spawnStrategy = StrategyFactory.CreateSpawnStrategy(data.spawnStrategy);
        moveStrategy = StrategyFactory.CreateMovementStrategy(data.moveStrategy);
        idleStrategy = StrategyFactory.CreatIdleStrategy(data.idleStrategy);
        skillStrategy = StrategyFactory.CreateSkillStrategy(data.skillStrategy, this);
        dieStrategy = StrategyFactory.CreatDieStrategy(data.dieStrategy);
        hitStrategy = StrategyFactory.CreatHitStrategy(data.hitStrategy);
        groggyStrategy = StrategyFactory.CreateGroggyStrategy(data.groggyStrategy, data);

        // 2. ���� Ư�� ������ �ʱ�ȭ
        if (data is BossData bossData)
        {
            // ������ �� ��� ���� �ʱ�ȭ
            currentPhaseStrategy = BossStrategyFactory.CreatePhaseTransitionStrategy(
                bossMonster.CurrentPhaseData.phaseTransitionType,
                bossMonster,
                this,
                bossUIManager
            );

            gimmickStrategy = BossStrategyFactory.CreateGimmickStrategy(
                bossMonster.CurrentPhaseGimmickData.type,
                this,
                bossMonster.CurrentPhaseGimmickData,
                bossMonster.CurrentPhaseGimmickData.hazardPrefab,
                bossData,
                successUI
            );

            if (miniGameManager != null)
            {
                // ������ �������� ������ globalMultiAttackStrategy�� ���
                // �ʱ� �������� �������� �߰�
                foreach (var strategyData in bossMonster.CurrentPhaseData.phaseAttackStrategies)
                {
                    var strategy = StrategyFactory.CreateAttackStrategy(strategyData.type, bossData, this);
                    if (strategy != null)
                    {
                        globalMultiAttackStrategy.AddStrategy(strategy, strategyData.weight);
                    }
                }
                attackStrategy = globalMultiAttackStrategy;
            }
            else
            {
                Debug.LogError("MiniGameManager is null!");
            }
        }

        // 3. ��ų ����Ʈ �ʱ�ȭ
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

        new BTSequence(this,
        new CheckGimmickNode(this),
        new ExecuteGimmickNode(this)
        ),
        // ���� ����
        new BTSequence(this,
            new CheckHealthCondition(this),
            new ChangeStateAction(this, MonsterStateType.Move, MovementStrategyType.Retreat)
        ),
        new TimeDelayDecorator(this,
        new BTSequence(this,
            new CheckPlayerInAttackRange(this),
            new CheckPatternDistance(this),
            new CombatDecisionNode(this)
        ),0.5f),
        new BTSequence(this,
            new CheckPlayerInRange(this),
            new ChangeStateAction(this, MonsterStateType.Move, MovementStrategyType.Basic)
        )
    );
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
    public override IGimmickStrategy GetGimmickStrategy() => gimmickStrategy;
    public override IPhaseTransitionStrategy GetPhaseTransitionStrategy() => currentPhaseStrategy;
    public override BossPattern GetBossPatternStartegy() => bossPatternStartaegy;


    public BossMonster GetBossMonster()
    {
        return bossMonster;
    }
    public override void SetAttackStrategy(IAttackStrategy newStrategy)
    {
        Debug.Log($"SetAttackStrategy called with {newStrategy.GetType().Name} from:\n{System.Environment.StackTrace}");
        attackStrategy = newStrategy;

        if (states == null)
        {
            Debug.Log("�����");
        }
        if (states.ContainsKey(MonsterStateType.Attack))
        {
            Debug.Log("���������� �ٲ�");
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