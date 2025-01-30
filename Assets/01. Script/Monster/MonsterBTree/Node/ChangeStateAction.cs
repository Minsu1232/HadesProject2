using UnityEngine;
using static IMonsterState;
public class ChangeStateAction : BTNode
{
    private MonsterStateType targetState;
    private MovementStrategyType? moveStrategy;  // Move ������ �� ����� ����

    // �⺻ ������
    public ChangeStateAction(CreatureAI owner, MonsterStateType state) : base(owner)
    {
        targetState = state;
    }

    // Move ���¸� ���� �߰� ������
    public ChangeStateAction(CreatureAI owner, MonsterStateType state, MovementStrategyType moveStrategy)
        : base(owner)
    {
        targetState = state;
        this.moveStrategy = moveStrategy;
    }

    public override NodeStatus Execute()
    {
        // Move �����̰� ������ �����Ǿ��ٸ� ���� ����
        if (targetState == MonsterStateType.Move && moveStrategy.HasValue)
        {
            owner.SetMovementStrategy(StrategyFactory.CreateMovementStrategy(moveStrategy.Value));
        }
        
        owner.ChangeState(targetState);
        return NodeStatus.Success;
    }
}