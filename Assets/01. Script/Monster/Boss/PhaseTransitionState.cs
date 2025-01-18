using static IMonsterState;
using UnityEngine;

public class PhaseTransitionState : MonsterBaseState
{
    private readonly IPhaseTransitionStrategy transitionStrategy;
    private Animator animator;

    public PhaseTransitionState(CreatureAI owner, IPhaseTransitionStrategy strategy) : base(owner)
    {
        transitionStrategy = strategy;
        animator = owner.GetComponent<Animator>();
    }

    public override void Enter()
    {
        animator.SetTrigger("PhaseTransition");
        transitionStrategy.StartTransition();
    }

    public override void Execute()
    {
        transitionStrategy.UpdateTransition();
        if (transitionStrategy.IsTransitionComplete)
        {
            owner.ChangeState(MonsterStateType.Idle);
        }
    }

    public override bool CanTransition() => transitionStrategy.IsTransitionComplete;
}