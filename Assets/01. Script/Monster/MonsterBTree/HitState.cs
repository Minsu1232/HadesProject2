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
            // �ǰ� ���°� ������ Move ���·� ��ȯ
            owner.ChangeState(MonsterStateType.Move);
        }
    }

    public override bool CanTransition()
    {
        return hitStrategy.IsHitComplete;
    }
}