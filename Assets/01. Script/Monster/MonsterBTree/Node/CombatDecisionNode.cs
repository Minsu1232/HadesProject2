// 공격 판단 노드
using static IMonsterState;
using UnityEngine;

public class CombatDecisionNode : BTNode
{
    private IAttackStrategy attackStrategy;
    private ISkillStrategy skillStrategy;

    public CombatDecisionNode(CreatureAI owner) : base(owner)
    {
        attackStrategy = owner.GetAttackStrategy();
        skillStrategy = owner.GetSkillStrategy();
    }

    public override NodeStatus Execute()
    {
        IMonsterClass monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(
            owner.transform.position,
            monster.GetPlayerPosition()
        );

        Debug.Log($"Combat Decision - Current State: {owner.GetCurrentState()}");

        if (skillStrategy.CanUseSkill(distanceToPlayer, monster))
        {
            Debug.Log("Changing to Skill state");
            owner.ChangeState(MonsterStateType.Skill);
            return NodeStatus.Success;
        }
        else if (attackStrategy.CanAttack(distanceToPlayer, monster))
        {
            Debug.Log("Changing to Attack state");
            owner.ChangeState(MonsterStateType.Attack);
            return NodeStatus.Success;
        }
        else if (distanceToPlayer <= monster.CurrentAttackRange)
        {
            Debug.Log("Changing to Idle state");
            owner.ChangeState(MonsterStateType.Idle);
            return NodeStatus.Success;
        }

        Debug.Log("No combat action possible");
        return NodeStatus.Failure;
    }
}