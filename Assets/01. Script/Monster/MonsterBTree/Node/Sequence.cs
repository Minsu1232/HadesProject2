using System.Collections.Generic;

public class Sequence : BTNode
{
    protected List<BTNode> children = new List<BTNode>();

    public Sequence(CreatureAI owner, params BTNode[] nodes) : base(owner)
    {
        children.AddRange(nodes);
    }

    public override NodeStatus Execute()
    {
        foreach (var child in children)
        {
            NodeStatus status = child.Execute();

            if (status == NodeStatus.Failure)
                return NodeStatus.Failure;  // 하나라도 실패하면 전체 실패

            if (status == NodeStatus.Running)
                return NodeStatus.Running;  // 실행 중이면 대기
        }
        return NodeStatus.Success;  // 모두 성공하면 성공
    }
}