using static IMonsterState;
using UnityEngine;

public class AttackState : MonsterBaseState
{
    private readonly IAttackStrategy attackStrategy;
    private Animator animator;
    private bool isFirstEnter = true;

    public AttackState(CreatureAI owner, IAttackStrategy strategy) : base(owner)
    {
        attackStrategy = strategy;
        animator = owner.GetComponent<Animator>();
    }

    public override void Enter()
    {
        // 상태 진입 시 새로운 패턴 선택 (BossMultiAttackStrategy인 경우에만)
        if (isFirstEnter && attackStrategy is BossMultiAttackStrategy bossStrategy)
        {
            bossStrategy.ChangePattern();
        }
        isFirstEnter = false;

        animator.SetTrigger(attackStrategy.GetAnimationTriggerName());
        attackStrategy.StartAttack();
    }

    public override void Execute()
    {
        float distanceToPlayer = GetDistanceToPlayer();

        // 공격 범위를 벗어났는지 체크
        if (!attackStrategy.CanAttack(distanceToPlayer, monsterClass))
        {
            attackStrategy.StopAttack();
            owner.ChangeState(MonsterStateType.Move);
            Debug.Log("Attack range exceeded, switching to Move state");
            return;
        }

        // 공격 실행
        attackStrategy.Attack(transform, player, monsterClass);
    }

    public override void Exit()
    {
        attackStrategy.StopAttack();
        animator.ResetTrigger(attackStrategy.GetAnimationTriggerName());
        isFirstEnter = true;  // 다음 진입을 위해 리셋
    }

    public override bool CanTransition()
    {
        return !attackStrategy.IsAttacking;
    }
}