using static IMonsterState;
using UnityEngine;

public class AttackState : MonsterBaseState
{
    private readonly IAttackStrategy attackStrategy;
    private Animator animator;
    public AttackState(CreatureAI owner, IAttackStrategy strategy) : base(owner)
    {
        attackStrategy = strategy;
        animator = owner.GetComponent<Animator>();
    }

    public override void Enter()
    {
        Debug.Log(attackStrategy.GetAnimationTriggerName());
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
            Debug.Log("멈춰!");
            return;
        }

   
        // 공격 실행
        
        attackStrategy.Attack(transform, player, monsterClass);

    }

    public override void Exit()
    {
        attackStrategy.StopAttack();
        animator.ResetTrigger(attackStrategy.GetAnimationTriggerName());
    }

    public override bool CanTransition()
    {// 그로기로 전환될 때는 즉시 허용
       
        return !attackStrategy.IsAttacking;
    }
}