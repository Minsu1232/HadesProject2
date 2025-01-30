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

        // ���� ������ ������� üũ
        if (!attackStrategy.CanAttack(distanceToPlayer, monsterClass))
        {   
            attackStrategy.StopAttack();
            
            owner.ChangeState(MonsterStateType.Move);
            Debug.Log("����!");
            return;
        }

   
        // ���� ����
        
        attackStrategy.Attack(transform, player, monsterClass);

    }

    public override void Exit()
    {
        attackStrategy.StopAttack();
        animator.ResetTrigger(attackStrategy.GetAnimationTriggerName());
    }

    public override bool CanTransition()
    {// �׷α�� ��ȯ�� ���� ��� ���
       
        return !attackStrategy.IsAttacking;
    }
}