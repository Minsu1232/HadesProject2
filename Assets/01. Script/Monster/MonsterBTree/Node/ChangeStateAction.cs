using UnityEngine;
using static IMonsterState;
public class ChangeStateAction : BTNode
{
    private MonsterStateType targetState;
    private MovementStrategyType? moveStrategy;  // Move 상태일 때 사용할 전략

    // 기본 생성자
    public ChangeStateAction(CreatureAI owner, MonsterStateType state) : base(owner)
    {
        targetState = state;
    }

    // Move 상태를 위한 추가 생성자
    public ChangeStateAction(CreatureAI owner, MonsterStateType state, MovementStrategyType moveStrategy)
        : base(owner)
    {
        targetState = state;
        this.moveStrategy = moveStrategy;
    }

    public override NodeStatus Execute()
    {
        // Move 상태이고 전략이 지정되었다면 전략 변경
        if (targetState == MonsterStateType.Move && moveStrategy.HasValue)
        {
            owner.SetMovementStrategy(StrategyFactory.CreateMovementStrategy(moveStrategy.Value));
        }
        
        owner.ChangeState(targetState);
        return NodeStatus.Success;
    }
}