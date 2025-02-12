using UnityEngine;
using static IMonsterState;

public class CheckPatternDistance : BTNode
{
    public CheckPatternDistance(CreatureAI owner) : base(owner) { }

    public override NodeStatus Execute()
    {
        var monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(
            owner.transform.position,
            monster.GetPlayerPosition()
        );

        // 거리가 너무 멀어졌다면
        if (distanceToPlayer > monster.CurrentAttackRange * 6f) 
        {
            Debug.Log("실패에");
            // 패턴 중단하고 이동 상태로
            owner.ChangeState(MonsterStateType.Move);
            return NodeStatus.Failure;
        }

        return NodeStatus.Success;
    }
}