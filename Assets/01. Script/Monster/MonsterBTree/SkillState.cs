using UnityEngine;
using static IMonsterState;

public class SkillState : MonsterBaseState
{
    private readonly ISkillStrategy skillStrategy;
    private Animator animator;
    public SkillState(CreatureAI owner, ISkillStrategy strategy) : base(owner)
    {
        skillStrategy = strategy;
        animator = owner.GetComponent<Animator>();
    }

    public override void Enter()
    {
        skillStrategy.StartSkill(transform, player, monsterClass);
        animator.SetTrigger("SkillAttack");
    }

    public override void Execute()
    {
        float distanceToPlayer = GetDistanceToPlayer();

        skillStrategy.UpdateSkill(transform, player, monsterClass);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        if (skillStrategy.IsSkillComplete)
        {
            // 스킬 완료 후 이동 상태로 전환
            owner.ChangeState(MonsterStateType.Move);
            animator.ResetTrigger("SkillAttack");
        }
    }

    public override bool CanTransition()
    {
        return !skillStrategy.IsUsingSkill;
    }
}