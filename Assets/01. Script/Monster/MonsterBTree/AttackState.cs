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

        // 공격 범위를 벗어났는지 체크
        if (!attackStrategy.CanAttack(distanceToPlayer, monsterClass))
        {   
            attackStrategy.StopAttack();
            
            owner.ChangeState(MonsterStateType.Move);
            return;
        }
   
        // 공격 실행
        
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