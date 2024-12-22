using static IMonsterState;

public class IdleState : MonsterBaseState
{
    private readonly IIdleStrategy idleStrategy;

    public IdleState(MonsterAI owner, IIdleStrategy strategy) : base(owner)
    {
        idleStrategy = strategy;
    }

    public override void Enter()
    {
        idleStrategy.OnIdle(transform, monsterClass);
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
        return true;  // Idle 상태는 언제든 전환 가능
    }
}