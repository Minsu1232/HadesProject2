// ���� �Ǵ� ���
using static IMonsterState;
using UnityEngine;

public class CombatDecisionNode : BTNode
{
    private IAttackStrategy attackStrategy;
    private ISkillStrategy skillStrategy;

    public CombatDecisionNode(MonsterAI owner) : base(owner)
    {
        // �������� �̹� MonsterAI���� �ʱ�ȭ�Ǿ� ���� ��
        attackStrategy = owner.GetAttackStrategy();
        skillStrategy = owner.GetSkillStrategy();
    }

    public override NodeStatus Execute()
    {
        MonsterClass monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(owner.transform.position, monster.GetPlayerPosition());
        
        // �� ������ �Ǵ� ���� ���
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