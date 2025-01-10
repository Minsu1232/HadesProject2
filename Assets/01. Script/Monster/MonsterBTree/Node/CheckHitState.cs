// �ǰ� ���� üũ ���
public class CheckHitState : BTNode
{
    public CheckHitState(MonsterAI owner) : base(owner) { }

    public override NodeStatus Execute()
    {
        // ���� �ǰ� ������ �������� üũ
        var currentState = owner.GetCurrentState();

        // �̹� �׾��ų� �ǰ� ���̸� ����
        if (currentState is DieState || currentState is HitState)
        {
            return NodeStatus.Failure;
        }

        return NodeStatus.Success;
    }
}