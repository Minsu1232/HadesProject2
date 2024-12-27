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

        
        if (distanceToPlayer <= monster.CurrentSkillRange)
        {
            return NodeStatus.Success;
        }

        return NodeStatus.Failure;
    }
}
