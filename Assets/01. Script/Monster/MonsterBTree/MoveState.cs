using static IMonsterState;
using UnityEngine;

public class MoveState : MonsterBaseState
{
    private IMovementStrategy currentStrategy;
    private Vector3 originPosition;
    private Animator animator;

    public MoveState(MonsterAI owner, IMovementStrategy strategy) : base(owner)
    {
        currentStrategy = strategy;
        originPosition = transform.position;
        animator = owner.GetComponent<Animator>();
    }

    public override void Execute()
    {
        animator.SetTrigger("Move");
        float distanceToPlayer = GetDistanceToPlayer();

        // ���� ��ȯ üũ
        if (currentStrategy.ShouldChangeState(distanceToPlayer, monsterClass))
        {
            if (distanceToPlayer <= monsterClass.CurrentAttackRange)
                owner.ChangeState(MonsterStateType.Attack);
            else if (distanceToPlayer > monsterClass.CurrentAggroDropRange)
                owner.ChangeState(MonsterStateType.Move);
            return;
        }

        // �̵� ����
        currentStrategy.Move(transform, player, monsterClass);
    }

    public override void Exit()
    {
        animator.ResetTrigger("Move");
        currentStrategy.StopMoving();
    }

    public override bool CanTransition()
    {
        return true;
    }
}