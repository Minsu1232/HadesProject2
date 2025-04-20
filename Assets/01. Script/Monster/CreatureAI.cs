using static AttackData;
using static IMonsterState;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class CreatureAI : MonoBehaviour, ICreatureAI
{
  
    public Animator animator;
    public ICreatureStatus creatureStatus;  // 공통 인터페이스 사용
    protected IMonsterClass monsterClass;
    protected Dictionary<IMonsterState.MonsterStateType, IMonsterState> states;
    protected IMonsterState currentState;

    protected ISpawnStrategy spawnStrategy;
    protected IMovementStrategy moveStrategy;
    protected IAttackStrategy attackStrategy;
    protected IIdleStrategy idleStrategy;
    protected ISkillStrategy skillStrategy;
    protected IDieStrategy dieStrategy;
    protected IHitStrategy hitStrategy;
    protected IGroggyStrategy groggyStrategy;
    protected BTNode behaviorTree;
    
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        creatureStatus = GetComponent<ICreatureStatus>();
        InitializeStates();
        creatureStatus.GetMonsterClass();
        monsterClass = creatureStatus.GetMonsterClass();
        
        monsterClass.OnArmorBreak += HandleArmorBreak;
      
    }
    private void HandleArmorBreak()
    {
        if (currentState.CanTransition())
        {
            ChangeState(MonsterStateType.Groggy);
        }
    }

    public ICreatureStatus GetStatus() => creatureStatus;
    protected abstract void InitializeStates();

    protected virtual void Update()
    {
        if (currentState != null)
        {
            currentState.Execute();
        }
        //if(behaviorTree != null)
        //{
        //    behaviorTree.Execute();
        //}
       

    }

    #region Core Methods
    public virtual void ChangeState(MonsterStateType newStateType)
    {

        // 현재 상태와 같은 상태로 변경하려고 하면 무시
        if (currentState != null && currentState == states[newStateType])
            return;

        if (currentState != null)
        {
            if (!currentState.CanTransition())
                return;

            currentState.Exit();
        }
        currentState = states[newStateType];
        currentState.Enter();
    }
    protected void ForceChangeState(MonsterStateType newStateType)
    {
        // 현재 상태와 같은 상태로 변경하려고 하면 무시
        if (currentState != null && currentState == states[newStateType])
            return;
   
        if (currentState != null)
        {
            // CanTransition 체크를 건너뛰고 강제로 상태 전환
            currentState.Exit();
        }

        currentState = states[newStateType];
        currentState.Enter();
    }
    public IMonsterState GetCurrentState() => currentState;

    //public MonsterStatus GetStatus() => monsterStatus;

    public void OnDamaged(int damage)
    {
        if (currentState is DieState)
            return;
        if (creatureStatus.GetMonsterClass().CurrentHealth <= 0)
        {
            ForceChangeState(MonsterStateType.Die);
            return;
        }
        if (currentState.CanTransition())
        {
            ChangeState(MonsterStateType.Hit);
        } 
   
       
    }
    public virtual void Die()
    {
        // 현재 상태가 이미 죽음인 경우 무시
        if (currentState is DieState)
            return;

        // 다른 상태의 전환 가능 여부와 관계없이 강제로 Death 상태로 전환
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = states[MonsterStateType.Die];
        currentState.Enter();
    }
    #endregion

    #region Strategy Getters
    public abstract IAttackStrategy GetAttackStrategy();
    public abstract ISkillStrategy GetSkillStrategy();
    public abstract ISpawnStrategy GetSpawnStrategy();
    public abstract IMovementStrategy GetMovementStrategy();
    public abstract IIdleStrategy GetIdleStrategy();
    public abstract IDieStrategy GetDieStrategy();
    public abstract IHitStrategy GetHitStrategy();
    public abstract IGroggyStrategy GetGroggyStrategy();
    public abstract IPhaseTransitionStrategy GetPhaseTransitionStrategy();
    public abstract IGimmickStrategy GetGimmickStrategy();
    public virtual BossPattern GetBossPatternStartegy()
    {
        return null;
    }
    #endregion

    // 애니메이션 이벤트 수신용 메서드들
    public void OnSkillStart()
    {
        (currentState as SkillState)?.OnSkillStart();
        
    }

    public void OnSkillEffect()
    {
        (currentState as SkillState)?.OnSkillEffect();
    }

    public void OnSkillAnimationComplete()
    {
        (currentState as SkillState)?.OnSkillAnimationComplete();
    }

    // 파티클 생성 메서드 추가
 
    #region Strategy Setters
    public abstract void SetMovementStrategy(IMovementStrategy newStrategy);
    public abstract void SetAttackStrategy(IAttackStrategy newStrategy);
    public abstract void SetSkillStrategy(ISkillStrategy newStrategy);
    public abstract void SetIdleStrategy(IIdleStrategy newStrategy);




    // 필요한 다른 Setter들도 추가 가능
    #endregion
}