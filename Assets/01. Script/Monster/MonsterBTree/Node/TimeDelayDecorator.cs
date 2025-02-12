using UnityEngine;

public class TimeDelayDecorator : BTNode
{
    private BTNode child;
    private float interval;
    private float lastExecuteTime;

    public TimeDelayDecorator(CreatureAI owner, BTNode child, float interval) : base(owner)
    {
        this.child = child;
        this.interval = interval;
        this.lastExecuteTime = -interval; // ù ������ ��� �ǵ���
    }

    public override NodeStatus Execute()
    {
        if (Time.time - lastExecuteTime < interval)
        {
            return NodeStatus.Running;
        }

        lastExecuteTime = Time.time;
        return child.Execute();
    }
}