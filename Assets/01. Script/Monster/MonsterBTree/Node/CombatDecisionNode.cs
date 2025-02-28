// 공격 판단 노드
using static IMonsterState;
using UnityEngine;

public class CombatDecisionNode : BTNode
{
    private IAttackStrategy attackStrategy;
    private ISkillStrategy skillStrategy;
    private BossPattern bossPatternStrategy;

    public CombatDecisionNode(CreatureAI owner) : base(owner)
    {
        attackStrategy = owner.GetAttackStrategy();
        skillStrategy = owner.GetSkillStrategy();
        bossPatternStrategy = owner.GetBossPatternStartegy();

        if (owner is BossAI bossAI)
        {
            bossAI.OnPatternChanged += UpdatePattern; // 추후 구독 해제 해줘야함
        }
    }
    private void UpdatePattern(BossPattern newPattern)
    {
        bossPatternStrategy = newPattern;
    }
    public override NodeStatus Execute()
    {
        IMonsterClass monster = owner.GetStatus().GetMonsterClass();
        float distanceToPlayer = Vector3.Distance(
            owner.transform.position,
            monster.GetPlayerPosition()
        );

        Debug.Log(skillStrategy.ToString());
        Debug.Log(skillStrategy.CanUseSkill(distanceToPlayer, monster));
        if (skillStrategy.CanUseSkill(distanceToPlayer, monster))
        {
            Debug.Log("스킬 가능합니다");
            owner.ChangeState(MonsterStateType.Skill);
            return NodeStatus.Success;
        }
        else if (bossPatternStrategy != null && bossPatternStrategy.CanAttack(distanceToPlayer, monster))
        {
            Debug.Log("가능합니다");
            owner.ChangeState(MonsterStateType.Pattern);
            return NodeStatus.Success;
        }
        else if (!attackStrategy.IsAttacking && attackStrategy.CanAttack(distanceToPlayer, monster))
        {
            Debug.Log("공격!가능합니다");
            owner.ChangeState(MonsterStateType.Attack);
            Debug.Log(owner.GetCurrentState()); 
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