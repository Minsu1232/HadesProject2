using System.Collections.Generic;

public class Selector : BTNode
{
    protected List<BTNode> children = new List<BTNode>();

    public Selector(CreatureAI owner, params BTNode[] nodes) : base(owner)
    {
        children.AddRange(nodes);
    }

    public override NodeStatus Execute()
    {
        foreach (var child in children)
        {
            NodeStatus status = child.Execute();

            if (status == NodeStatus.Success)
                return NodeStatus.Success;  // �ϳ��� �����ϸ� ��ü ����

            if (status == NodeStatus.Running)
                return NodeStatus.Running;  // ���� ���̸� ���
        }
        return NodeStatus.Failure;  // ��� �����ϸ� ����
    }
}