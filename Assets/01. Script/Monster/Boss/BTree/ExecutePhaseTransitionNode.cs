using static IMonsterState;

public class ExecutePhaseTransitionNode : BTNode
{
    private BossMonster boss;
    private IPhaseTransitionStrategy transitionStrategy;

    public ExecutePhaseTransitionNode(CreatureAI owner) : base(owner)
    {
        boss = owner.GetStatus().GetMonsterClass() as BossMonster;
        transitionStrategy = owner.GetPhaseTransitionStrategy();
    }

    public override NodeStatus Execute()
    {
        if (boss == null)
            return NodeStatus.Failure;

        if (!boss.IsInPhaseTransition)
        {
            owner.ChangeState(MonsterStateType.PhaseTransition);
        }

        return NodeStatus.Success;
    }
}