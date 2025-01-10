using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayerInAttackRange : BTNode
{
    public CheckPlayerInAttackRange(MonsterAI owner) : base(owner) { }

    public override NodeStatus Execute()
    {
        MonsterClass monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(
            owner.transform.position,
            owner.GetStatus().GetMonsterClass().GetPlayerPosition()
        );

        // 스킬 범위나 공격 범위 중 하나라도 안에 있으면 Success
        if (distanceToPlayer <= monster.CurrentSkillRange ||
            distanceToPlayer <= monster.CurrentAttackRange)
        {
            return NodeStatus.Success;
        }
        return NodeStatus.Failure;
    }
}
