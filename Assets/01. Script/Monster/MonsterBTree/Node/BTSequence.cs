using System.Collections.Generic;

public class BTSequence : BTNode
{
    protected List<BTNode> children = new List<BTNode>();

    public BTSequence(CreatureAI owner, params BTNode[] nodes) : base(owner)
    {
        children.AddRange(nodes);
    }

    public override NodeStatus Execute()
    {
        foreach (var child in children)
        {
            NodeStatus status = child.Execute();

            if (status == NodeStatus.Failure)
                return NodeStatus.Failure;  // �ϳ��� �����ϸ� ��ü ����

            if (status == NodeStatus.Running)
                return NodeStatus.Running;  // ���� ���̸� ���
        }
        return NodeStatus.Success;  // ��� �����ϸ� ����
    }
}