using static AttackData;
using static IMonsterState;
using System.Collections.Generic;
using UnityEngine;

public abstract class CreatureAI : MonoBehaviour, ICreatureAI
{
    public Animator animator;
    protected ICreatureStatus creatureStatus;  // ���� �������̽� ���

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
        IMonsterClass monsterClass = creatureStatus.GetMonsterClass();
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
        Debug.Log(currentState);

    }

    #region Core Methods
    public virtual void ChangeState(MonsterStateType newStateType)
    {
       
        
        if (currentState != null)
        {
            
            if (!currentState.CanTransition())
                return;
            Debug.Log("out");
            currentState.Exit();
        }

        currentState = states[newStateType];
        currentState.Enter();
    }

    public IMonsterState GetCurrentState() => currentState;

    //public MonsterStatus GetStatus() => monsterStatus;

    public virtual void OnDamaged(int damage)
    {
        if (currentState is DieState)
            return;

        if (currentState.CanTransition())
        {
            ChangeState(MonsterStateType.Hit);
        }
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
    #endregion

    // �ִϸ��̼� �̺�Ʈ ���ſ� �޼����
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

    // ��ƼŬ ���� �޼��� �߰�
 
    #region Strategy Setters
    public abstract void SetMovementStrategy(IMovementStrategy newStrategy);
    public abstract void SetAttackStrategy(IAttackStrategy newStrategy);
    public abstract void SetSkillStrategy(ISkillStrategy newStrategy);
    public abstract void SetIdleStrategy(IIdleStrategy newStrategy);
    // �ʿ��� �ٸ� Setter�鵵 �߰� ����
    #endregion
}