using UnityEngine;
using static IMonsterState;

public class IdleState : MonsterBaseState
{
    private readonly IIdleStrategy idleStrategy;
    private Animator animator;
    public IdleState(CreatureAI owner, IIdleStrategy strategy) : base(owner)
    {
        idleStrategy = strategy;
        animator = owner.GetComponent<Animator>();
    }

    public override void Enter()
    {
        idleStrategy.OnIdle(transform, monsterClass);
        animator.SetTrigger("Idle");
        
    }

    public override void Execute()
    {
        float distanceToPlayer = GetDistanceToPlayer();

        idleStrategy.UpdateIdle();

        if (idleStrategy.ShouldChangeState(distanceToPlayer, monsterClass))
        {
            owner.ChangeState(MonsterStateType.Move);
        }
    }

    public override bool CanTransition()
    {
        animator.ResetTrigger("Idle");
        return true;  // Idle 상태는 언제든 전환 가능
    }
}