// 공격 판단 노드
using static IMonsterState;
using UnityEngine;

public class CombatDecisionNode : BTNode
{
    private IAttackStrategy attackStrategy;
    private ISkillStrategy skillStrategy;

    public CombatDecisionNode(MonsterAI owner) : base(owner)
    {
        // 전략들은 이미 MonsterAI에서 초기화되어 있을 것
        attackStrategy = owner.GetAttackStrategy();
        skillStrategy = owner.GetSkillStrategy();
    }

    public override NodeStatus Execute()
    {
        MonsterClass monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(owner.transform.position, monster.GetPlayerPosition());
        
        // 각 전략의 판단 로직 사용
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

        return NodeStatus.Failure;
    }
}