public enum NodeStatus
{
    Success,
    Failure,
    Running
}

public abstract class BTNode
{
    protected MonsterAI owner;

    public BTNode(MonsterAI owner)
    {
        this.owner = owner;
    }

    public abstract NodeStatus Execute();
}