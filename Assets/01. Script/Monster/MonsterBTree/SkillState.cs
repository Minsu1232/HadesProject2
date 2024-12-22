using static IMonsterState;

public class SkillState : MonsterBaseState
{
    private readonly ISkillStrategy skillStrategy;

    public SkillState(MonsterAI owner, ISkillStrategy strategy) : base(owner)
    {
        skillStrategy = strategy;
    }

    public override void Enter()
    {
        skillStrategy.StartSkill(transform, player, monsterClass);
    }

    public override void Execute()
    {
        float distanceToPlayer = GetDistanceToPlayer();

        skillStrategy.UpdateSkill(transform, player, monsterClass);

        if (skillStrategy.IsSkillComplete)
        {
            // ��ų �Ϸ� �� �̵� ���·� ��ȯ
            owner.ChangeState(MonsterStateType.Move);
        }
    }

    public override bool CanTransition()
    {
        return !skillStrategy.IsUsingSkill;
    }
}