using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayerInAttackRange : BTNode
{
    public CheckPlayerInAttackRange(CreatureAI owner) : base(owner) { }

    public override NodeStatus Execute()
    {
        MonsterClass monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(
            owner.transform.position,
            owner.GetStatus().GetMonsterClass().GetPlayerPosition()
        );

        Debug.Log($"Distance to player: {distanceToPlayer}");
        Debug.Log($"Attack range: {monster.CurrentAttackRange}");
        Debug.Log($"Skill range: {monster.CurrentSkillRange}");

        if (distanceToPlayer <= monster.CurrentAttackRange ||
            distanceToPlayer <= monster.CurrentSkillRange)
        {
            Debug.Log("Player in attack/skill range!");
            return NodeStatus.Success;
        }

        Debug.Log("Player out of range");
        return NodeStatus.Failure;
    }
}
