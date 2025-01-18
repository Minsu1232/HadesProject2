using UnityEngine;

public class CheckPlayerInRange : BTNode
{
    public CheckPlayerInRange(CreatureAI owner) : base(owner) { }

    public override NodeStatus Execute()
    {
        MonsterClass monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(
            owner.transform.position,
            owner.GetStatus().GetMonsterClass().GetPlayerPosition()
        );

        // 추적 범위 안에 있는지 체크
        if (distanceToPlayer <= monster.CurrentChaseRange)
        {
            return NodeStatus.Success;
        }

        return NodeStatus.Failure;
    }
}