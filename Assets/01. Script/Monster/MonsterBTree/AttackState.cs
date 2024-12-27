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
            Debug.Log("���� ���� ���");
            owner.ChangeState(MonsterStateType.Move);
            return;
        }
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        // ���� ����
        Debug.Log("���� ����");
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