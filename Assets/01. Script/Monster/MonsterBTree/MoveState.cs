using static IMonsterState;
using UnityEngine;

public class MoveState : MonsterBaseState
{
    private IMovementStrategy currentStrategy;
    private Vector3 originPosition;
    private Animator animator;
    private Vector3 lastPosition;  // ���� �������� ��ġ�� ����

    public MoveState(MonsterAI owner, IMovementStrategy strategy) : base(owner)
    {
        currentStrategy = strategy;
        originPosition = transform.position;
        animator = owner.GetComponent<Animator>();
        lastPosition = transform.position;
    }

    public override void Execute()
    {
        float distanceToPlayer = GetDistanceToPlayer();

        // ���� �̵� ���� ���
        Vector3 moveDirection = (transform.position - lastPosition).normalized;
        if (moveDirection != Vector3.zero)  // ������ �̵� ���� ���� ȸ��
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // ���� ��ġ ����
        lastPosition = transform.position;

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