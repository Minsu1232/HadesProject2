using static IMonsterState;

public class ExecuteGimmickNode : BTNode
{
    private BossMonster boss;
    private IGimmickStrategy gimmickStrategy;

    public ExecuteGimmickNode(CreatureAI owner) : base(owner)
    {
        boss = owner.GetStatus().GetMonsterClass() as BossMonster;
        gimmickStrategy = owner.GetGimmickStrategy();
    }

    public override NodeStatus Execute()
    {
        if (boss == null)
            return NodeStatus.Failure;

        if (!boss.IsInGimmick)  // IsInGimmick 같은 플래그 필요
        {
            owner.ChangeState(MonsterStateType.Gimmick);
        }

        return NodeStatus.Success;
    }
}