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


        
        if (skillStrategy.CanUseSkill(distanceToPlayer, monster))
        {
            owner.ChangeState(MonsterStateType.Skill);
            return NodeStatus.Success;
        }
        else if (attackStrategy.CanAttack(distanceToPlayer, monster))
        {
            
            owner.ChangeState(MonsterStateType.Attack);
            return NodeStatus.Success;
        }
        else if (distanceToPlayer <= monster.CurrentAttackRange)
        {
           
            owner.ChangeState(MonsterStateType.Idle);
            return NodeStatus.Success;
        }

       
        return NodeStatus.Failure;
    }
}