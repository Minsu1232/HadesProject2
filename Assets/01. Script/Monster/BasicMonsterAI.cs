using DG.Tweening;
using static IMonsterState;
using System.Collections.Generic;
using UnityEngine;

public class BasicCreatureAI : CreatureAI
{
    // 전략들을 전역 변수로 관리


    private void Awake()
    {
        DOTween.Init();
        DOTween.SetTweensCapacity(500, 50);
    }
    protected override void Start()
    {
        base.Start();

       
    }

   
   
    protected override void InitializeStates()
    {
        IMonsterClass monsterClass = creatureStatus.GetMonsterClass();
        ICreatureData data = monsterClass.GetMonsterData();

        // 데이터의 전략 타입에 따라 전략 생성
        InitializeStrategies(data);
        InitializeSkillEffect(data);
        InitializeBehaviorTree();

        // 생성된 전략으로 상태 초기화
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

        // 초기 상태 설정
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

    private void InitializeBehaviorTree()
    {
        behaviorTree = new Selector(this,
           // 1. 체력이 낮으면 도망
           new BTSequence(this,
               new CheckHealthCondition(this),
               new ChangeStateAction(this, MonsterStateType.Move, MovementStrategyType.Retreat)
           ),

           // 2. 전투 가능 거리면 전투
           new BTSequence(this,
               new CheckPlayerInAttackRange(this),
               new CombatDecisionNode(this)
           ),

           // 3. 감지 범위 안이면 추적
           new BTSequence(this,
               new CheckPlayerInRange(this),
               new ChangeStateAction(this, MonsterStateType.Move, MovementStrategyType.Basic)
           )
       );
    }

    protected override void Update()
    {
        base.Update();
        behaviorTree?.Execute();
    }

    #region Strategy Getters
    public override IAttackStrategy GetAttackStrategy() => attackStrategy;
    public override ISkillStrategy GetSkillStrategy() => skillStrategy;
    public override ISpawnStrategy GetSpawnStrategy() => spawnStrategy;
    public override IMovementStrategy GetMovementStrategy() => moveStrategy;
    public override IIdleStrategy GetIdleStrategy() => idleStrategy;
    public override IDieStrategy GetDieStrategy() => dieStrategy;
    public override IHitStrategy GetHitStrategy() => hitStrategy;
    public override IGroggyStrategy GetGroggyStrategy() => groggyStrategy;
    #endregion

    #region Strategy Setters
    public override void SetMovementStrategy(IMovementStrategy newStrategy)
    {
        moveStrategy = newStrategy;
        if (states.ContainsKey(MonsterStateType.Move))
        {
            states[MonsterStateType.Move] = new MoveState(this, moveStrategy);
        }
    }

    public override void SetAttackStrategy(IAttackStrategy newStrategy)
    {
        attackStrategy = newStrategy;
        if (states.ContainsKey(MonsterStateType.Attack))
        {
            states[MonsterStateType.Attack] = new AttackState(this, attackStrategy);
        }
    }
    #endregion

    public void OnAttackAnimationEnd()
    {
        attackStrategy.OnAttackAnimationEnd();
    }

    public override void SetSkillStrategy(ISkillStrategy newStrategy)
    {
        throw new System.NotImplementedException();
    }

    public override void SetIdleStrategy(IIdleStrategy newStrategy)
    {
        throw new System.NotImplementedException();
    }

    public override IPhaseTransitionStrategy GetPhaseTransitionStrategy()
    {
        throw new System.NotImplementedException();
    }
}