using UnityEngine;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using static IMonsterState;

public class MonsterAI : MonoBehaviour
{
    private Dictionary<IMonsterState.MonsterStateType, IMonsterState> states;
    private IMonsterState currentState;
    private MonsterStatus monsterStatus;

    private void Start()
    {
        monsterStatus = GetComponent<MonsterStatus>();
        
        InitializeStates();
    }

    private void InitializeStates()
    {
        MonsterClass monsterClass = monsterStatus.GetMonsterClass();
        MonsterData data = monsterClass.GetMonsterData();

        // �������� ���� Ÿ�Կ� ���� ���� ����
        ISpawnStrategy spawnStrategy = StrategyFactory.CreateSpawnStrategy(data.spawnStrategy);
        IMovementStrategy moveStrategy = StrategyFactory.CreateMovementStrategy(data.moveStrategy);
        IAttackStrategy attackStrategy = StrategyFactory.CreateAttackStrategy(data.attackStrategy);
        IIdleStrategy idleStrategy = StrategyFactory.CreatIdleStrategy(data.idleStrategy);
        ISkillStrategy skillStrategy = StrategyFactory.CreateSkillStrategy(data.skillStrategy);
        IDieStrategy dieStrategy = StrategyFactory.CreatDieStrategy(data.dieStrategy);
        IHitStrategy hitStrategy = StrategyFactory.CreatHitStrategy(data.hitStrategy);

        // ������ �������� ���� �ʱ�ȭ
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

        // �ʱ� ���� ����
        ChangeState(MonsterStateType.Spawn);
    }

    private void Update()
    {
        if (currentState != null)
        {
            Debug.Log(currentState);
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
}