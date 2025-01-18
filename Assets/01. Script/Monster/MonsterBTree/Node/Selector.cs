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
                return NodeStatus.Success;  // 하나라도 성공하면 전체 성공

            if (status == NodeStatus.Running)
                return NodeStatus.Running;  // 실행 중이면 대기
        }
        return NodeStatus.Failure;  // 모두 실패하면 실패
    }
}