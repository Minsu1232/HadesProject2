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
        MonsterClass monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(owner.transform.position, monster.GetPlayerPosition());

        // 스킬이나 공격이 가능한 상태인지 확인
        if (skillStrategy.CanUseSkill(distanceToPlayer, monster))
        {
            Debug.Log("스킬 사용 가능!");
            Debug.Log($"isUsingSkill: {skillStrategy.IsUsingSkill}");
            Debug.Log($"쿨타임 체크: {Time.time > skillStrategy.GetLastSkillTime + monster.CurrentSkillCooldown}");
            Debug.Log($"범위 체크: {distanceToPlayer <= skillStrategy.SkillRange}");
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
            // 공격 범위 안이지만 스킬/공격 불가능(쿨타임)한 경우
            owner.ChangeState(MonsterStateType.Idle);
            
            return NodeStatus.Success;
        }

        return NodeStatus.Failure;
    }
}