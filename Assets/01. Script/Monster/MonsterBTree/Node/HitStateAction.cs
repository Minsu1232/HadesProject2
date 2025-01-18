// �ǰ� ���� ��ȯ �׼� ���
using static IMonsterState;

public class HitStateAction : BTNode
{
    public HitStateAction(CreatureAI owner) : base(owner) { }

    public override NodeStatus Execute()
    {
        // ���� ���°� �̹� HitState���� Ȯ��
        if (owner.GetCurrentState() is HitState)
        {
            // �ǰ� ������ �Ϸ� ���� Ȯ��
            if (owner.GetHitStrategy().IsHitComplete)
            {
                return NodeStatus.Success;
            }
            return NodeStatus.Running;
        }

        // Hit ���·� ��ȯ �õ�
        owner.ChangeState(MonsterStateType.Hit);
        return NodeStatus.Running;
    }
}
