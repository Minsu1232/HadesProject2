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
        // ���� ���� �� ���ο� ���� ���� (BossMultiAttackStrategy�� ��쿡��)
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

        // ���� ������ ������� üũ
        if (!attackStrategy.CanAttack(distanceToPlayer, monsterClass))
        {
            attackStrategy.StopAttack();
            owner.ChangeState(MonsterStateType.Move);
            Debug.Log("Attack range exceeded, switching to Move state");
            return;
        }

        // ���� ����
        attackStrategy.Attack(transform, player, monsterClass);
    }

    public override void Exit()
    {
        attackStrategy.StopAttack();
        animator.ResetTrigger(attackStrategy.GetAnimationTriggerName());
        isFirstEnter = true;  // ���� ������ ���� ����
    }

    public override bool CanTransition()
    {
        return !attackStrategy.IsAttacking;
    }
}