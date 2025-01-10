using UnityEngine;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using static IMonsterState;
using System;
using static AttackData;

public class MonsterAI : MonoBehaviour
{
    private Dictionary<IMonsterState.MonsterStateType, IMonsterState> states;
    private IMonsterState currentState;
    private MonsterStatus monsterStatus;
    private BTNode behaviorTree;
    // 전략들을 전역 변수로 관리
    private ISpawnStrategy spawnStrategy;
    private IMovementStrategy moveStrategy;
    private IAttackStrategy attackStrategy;
    private IIdleStrategy idleStrategy;
    private ISkillStrategy skillStrategy;
    private IDieStrategy dieStrategy;
    private IHitStrategy hitStrategy;


    


    private void Start()
    {
        monsterStatus = GetComponent<MonsterStatus>();
        
        InitializeStates();
        InitializeBehaviorTree();  // 추가
    }

    private void InitializeStates()
    {
        MonsterClass monsterClass = monsterStatus.GetMonsterClass();
        MonsterData data = monsterClass.GetMonsterData();
        
        // 데이터의 전략 타입에 따라 전략 생성
        spawnStrategy = StrategyFactory.CreateSpawnStrategy(data.spawnStrategy);
        moveStrategy = StrategyFactory.CreateMovementStrategy(data.moveStrategy);
        attackStrategy = StrategyFactory.CreateAttackStrategy(data.attackStrategy);
        idleStrategy = StrategyFactory.CreatIdleStrategy(data.idleStrategy);
         skillStrategy = StrategyFactory.CreateSkillStrategy(data.skillStrategy,this);
         dieStrategy = StrategyFactory.CreatDieStrategy(data.dieStrategy);
         hitStrategy = StrategyFactory.CreatHitStrategy(data.hitStrategy);

        ISkillEffect skillEffect = StrategyFactory.CreateSkillEffect(
        data.skillEffectType,
        data,
        this  // MonsterAI 인스턴스 전달
    );

        if (skillEffect != null)
        {
            skillStrategy.Initialize(skillEffect);
            skillStrategy.SkillRange = data.skillRange;
        }
        else
        {
            Debug.LogError($"Failed to create skill effect for monster: {data.monsterName}");
        }
        // 생성된 전략으로 상태 초기화
        states = new Dictionary<IMonsterState.MonsterStateType, IMonsterState>
        {
            { MonsterStateType.Spawn, new SpawnState(this, spawnStrategy) },
            { MonsterStateType.Idle, new IdleState(this, idleStrategy) },
            { MonsterStateType.Move, new MoveState(this, moveStrategy) },
            { MonsterStateType.Attack, new AttackState(this, attackStrategy) },
            { MonsterStateType.Skill, new SkillState(this, skillStrategy) },
            { MonsterStateType.Hit, new HitState(this, hitStrategy) },
            { MonsterStateType.Die, new DieState(this, dieStrategy) }
        };

        // 초기 상태 설정
        ChangeState(MonsterStateType.Spawn);
    }
    public IAttackStrategy GetAttackStrategy() => attackStrategy;
    public ISkillStrategy GetSkillStrategy() => skillStrategy;

    public ISpawnStrategy GetSpawnStrategy() => spawnStrategy;

    public IMovementStrategy GetMovementStrategy() => moveStrategy;
    public IIdleStrategy GetIdleStrategy() => idleStrategy;
    public IDieStrategy GetDieStrategy() => dieStrategy;
    public IHitStrategy GetHitStrategy() => hitStrategy;
    // 전략 설정 메서드들 추가
    public void SetMovementStrategy(IMovementStrategy newStrategy)
    {
        moveStrategy = newStrategy;
        // 현재 Move 상태라면 상태 객체도 업데이트
        if (states.ContainsKey(MonsterStateType.Move))
        {
            states[MonsterStateType.Move] = new MoveState(this, moveStrategy);
        }
    }

    public void SetAttackStrategy(IAttackStrategy newStrategy)
    {
        attackStrategy = newStrategy;
        if (states.ContainsKey(MonsterStateType.Attack))
        {
            states[MonsterStateType.Attack] = new AttackState(this, attackStrategy);
        }
    }
    private void InitializeBehaviorTree()
    {
        behaviorTree = new Selector(this,
           // 1. 체력이 낮으면 도망
           new Sequence(this,
               new CheckHealthCondition(this),
               new ChangeStateAction(this, MonsterStateType.Move, MovementStrategyType.Retreat)
           ),

           // 2. 전투 가능 거리면 전투
           new Sequence(this,
               new CheckPlayerInAttackRange(this),
               new CombatDecisionNode(this)
           ),

           // 3. 감지 범위 안이면 추적
           new Sequence(this,
               new CheckPlayerInRange(this),
               new ChangeStateAction(this, MonsterStateType.Move, MovementStrategyType.Basic)
           )
       );
    }
    public IMonsterState GetCurrentState()
    {
        return currentState;
    }
    public void OnDamaged(int damage, AttackType attackType)
    {
        
        if (currentState is DieState)
            return;
       
        if (currentState.CanTransition())
        {
                ChangeState(MonsterStateType.Hit);       
            
        }
    }
    private void Update()
    {
        behaviorTree.Execute();  // 행동 트리 실행
        
        if (currentState != null)
        {
            
            currentState.Execute();            
        }
    }

    public void ChangeState(MonsterStateType newStateType)
    {
        
        if (currentState != null)
        {
            
            if (!currentState.CanTransition())
                return;

            currentState.Exit();
        }

        currentState = states[newStateType];
        currentState.Enter();

       
    }

    public MonsterStatus GetStatus()
    {
        return monsterStatus;
    }

    public void OnAttackAnimationEnd()
    {
        attackStrategy.OnAttackAnimationEnd();
    }
    // PlayerAttack의 OnAttackInput 이벤트가 발생하면 이 함수가 호출됨
  
  
}