using static IMonsterState;
using UnityEngine;

public class AttackState : MonsterBaseState
{
    private readonly IAttackStrategy attackStrategy;
    private Animator animator;
    public AttackState(MonsterAI owner, IAttackStrategy strategy) : base(owner)
    {
        attackStrategy = strategy;
        animator = owner.GetComponent<Animator>();
    }

    public override void Enter()
    {
        animator.SetTrigger("Attack");
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
            return;
        }
   
        // ���� ����
        
        attackStrategy.Attack(transform, player, monsterClass);

    }

    public override void Exit()
    {
        attackStrategy.StopAttack();
        animator.ResetTrigger("Attack");
    }

    public override bool CanTransition()
    {
        return !attackStrategy.IsAttacking;
    }
}