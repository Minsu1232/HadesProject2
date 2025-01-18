public enum NodeStatus
{
    Success,
    Failure,
    Running
}

public abstract class BTNode
{
    protected CreatureAI owner;

    public BTNode(CreatureAI owner)
    {
        this.owner = owner;
    }

    public abstract NodeStatus Execute();
}