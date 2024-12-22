using static IMonsterState;

public class HitState : MonsterBaseState
{
    private readonly IHitStrategy hitStrategy;
    private int damageAmount;

    public HitState(MonsterAI owner, IHitStrategy strategy) : base(owner)
    {
        hitStrategy = strategy;
    }

    public void SetDamage(int damage)
    {
        this.damageAmount = damage;
    }

    public override void Enter()
    {
        hitStrategy.OnHit(transform, monsterClass, damageAmount);
    }

    public override void Execute()
    {
        hitStrategy.UpdateHit();

        if (hitStrategy.IsHitComplete)
        {
            // 피격 상태가 끝나면 Move 상태로 전환
            owner.ChangeState(MonsterStateType.Move);
        }
    }

    public override bool CanTransition()
    {
        return hitStrategy.IsHitComplete;
    }
}