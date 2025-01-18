// ���� �Ǵ� ���
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
        MonsterClass monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(owner.transform.position, monster.GetPlayerPosition());

        // ��ų�̳� ������ ������ �������� Ȯ��
        if (skillStrategy.CanUseSkill(distanceToPlayer, monster))
        {
            Debug.Log("��ų ��� ����!");
            Debug.Log($"isUsingSkill: {skillStrategy.IsUsingSkill}");
            Debug.Log($"��Ÿ�� üũ: {Time.time > skillStrategy.GetLastSkillTime + monster.CurrentSkillCooldown}");
            Debug.Log($"���� üũ: {distanceToPlayer <= skillStrategy.SkillRange}");
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
            // ���� ���� �������� ��ų/���� �Ұ���(��Ÿ��)�� ���
            owner.ChangeState(MonsterStateType.Idle);
            
            return NodeStatus.Success;
        }

        return NodeStatus.Failure;
    }
}