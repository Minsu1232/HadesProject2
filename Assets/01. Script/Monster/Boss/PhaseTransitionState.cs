using static IMonsterState;
using UnityEngine;

public class PhaseTransitionState : MonsterBaseState
{
    private  IPhaseTransitionStrategy transitionStrategy;
    private Animator animator;
    private BossMonster boss;

    public PhaseTransitionState(CreatureAI owner, IPhaseTransitionStrategy strategy,BossMonster bossMonster) : base(owner)
    {
        this.transitionStrategy = strategy;
        animator = owner.GetComponent<Animator>();
        this.boss = bossMonster;
    }
    public void UpdateStrategy(IPhaseTransitionStrategy newStrategy)
    {
       this.transitionStrategy = newStrategy;
    }
    public override void Enter()
    {
        animator.SetBool("IsTransition",true);
        boss.SetInPhaseTransition(true);  // 추가
        transitionStrategy.StartTransition();
    }

    public override void Execute()
    {
        transitionStrategy.UpdateTransition();
        
        if (transitionStrategy.IsTransitionComplete)
        {
            Debug.Log("페이즈변환끝" + transitionStrategy.IsTransitionComplete);
            animator.SetBool("IsTransition", false);
            
            boss.SetInPhaseTransition(false);  // 추가
            
            owner.ChangeState(MonsterStateType.Idle);
           
        }
    }

    public override bool CanTransition() => transitionStrategy.IsTransitionComplete;
}