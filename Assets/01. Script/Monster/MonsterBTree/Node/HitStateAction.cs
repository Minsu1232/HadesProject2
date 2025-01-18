// 피격 상태 전환 액션 노드
using static IMonsterState;

public class HitStateAction : BTNode
{
    public HitStateAction(CreatureAI owner) : base(owner) { }

    public override NodeStatus Execute()
    {
        // 현재 상태가 이미 HitState인지 확인
        if (owner.GetCurrentState() is HitState)
        {
            // 피격 전략의 완료 여부 확인
            if (owner.GetHitStrategy().IsHitComplete)
            {
                return NodeStatus.Success;
            }
            return NodeStatus.Running;
        }

        // Hit 상태로 전환 시도
        owner.ChangeState(MonsterStateType.Hit);
        return NodeStatus.Running;
    }
}
