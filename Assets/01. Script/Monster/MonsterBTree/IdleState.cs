using UnityEngine;

public class IdleState : MonsterBaseState
{
    private readonly IIdleStrategy idleStrategy;
    private Animator animator;
    private bool isTransitioning = false;

    public IdleState(CreatureAI owner, IIdleStrategy strategy) : base(owner)
    {
        idleStrategy = strategy;
        animator = owner.GetComponent<Animator>();
    }

    public override void Enter()
    {
        isTransitioning = false;
        idleStrategy.OnIdle(transform, monsterClass);
        animator.SetTrigger("Idle");
    }

    public override void Execute()
    {
        idleStrategy.UpdateIdle();
    }

    public override void Exit()
    {
        isTransitioning = true;
        animator.ResetTrigger("Idle");
    }

    public override bool CanTransition()
    {
        return true;
    }
}
