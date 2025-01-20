using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayerInAttackRange : BTNode
{
    public CheckPlayerInAttackRange(CreatureAI owner) : base(owner) { }

    public override NodeStatus Execute()
    {
        IMonsterClass monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(
            owner.transform.position,
            owner.GetStatus().GetMonsterClass().GetPlayerPosition()
        );

   

        if (distanceToPlayer <= monster.CurrentAttackRange ||
            distanceToPlayer <= monster.CurrentSkillRange)
        {
            
            return NodeStatus.Success;
        }

        
        return NodeStatus.Failure;
    }
}
