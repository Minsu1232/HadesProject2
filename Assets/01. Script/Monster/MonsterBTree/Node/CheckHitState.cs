// 피격 상태 체크 노드
public class CheckHitState : BTNode
{
    public CheckHitState(MonsterAI owner) : base(owner) { }

    public override NodeStatus Execute()
    {
        // 현재 피격 가능한 상태인지 체크
        var currentState = owner.GetCurrentState();

        // 이미 죽었거나 피격 중이면 실패
        if (currentState is DieState || currentState is HitState)
        {
            return NodeStatus.Failure;
        }

        return NodeStatus.Success;
    }
}