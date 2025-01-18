using static IMonsterState;
using UnityEngine;

public class MoveState : MonsterBaseState
{
    private IMovementStrategy currentStrategy;
    private Vector3 originPosition;
    private Animator animator;

    public MoveState(CreatureAI owner, IMovementStrategy strategy) : base(owner)
    {
        currentStrategy = strategy;
        originPosition = transform.position;
        animator = owner.GetComponent<Animator>();
    }

    public override void Execute()
    {
        animator.SetTrigger("Move");
        float distanceToPlayer = GetDistanceToPlayer();

        // ���� �����Ÿ� �ȿ� ������ Idle ���·� ��ȯ
        if (distanceToPlayer <= monsterClass.CurrentAttackRange)
        {
            
            owner.ChangeState(MonsterStateType.Idle);
            return;
        }

        // ���� ���� ��ȯ üũ�� �̵� ����
        if (currentStrategy.ShouldChangeState(distanceToPlayer, monsterClass))
        {
            if (distanceToPlayer > monsterClass.CurrentAggroDropRange)
                owner.ChangeState(MonsterStateType.Move);
            return;
        }

        currentStrategy.Move(transform, player, monsterClass);
    }

    public override void Exit()
    {
        animator.ResetTrigger("Move");
        currentStrategy.StopMoving();
    }

    public override bool CanTransition()
    {
        float distanceToPlayer = GetDistanceToPlayer();

        // ���� �����Ÿ� �ȿ� ������ �̵� ���� ��ȯ�� ���� > �����ؾ���
        //if (distanceToPlayer <= monsterClass.CurrentAttackRange)
        //{
        //    Debug.Log("������");
        //    return false;
        //}
     

        return true;
    }
}