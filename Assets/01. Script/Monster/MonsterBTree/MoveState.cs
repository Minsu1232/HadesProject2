using static IMonsterState;
using UnityEngine;

public class MoveState : MonsterBaseState
{   
    private IMovementStrategy currentStrategy;
    private Vector3 originPosition;

    public MoveState(MonsterAI owner, IMovementStrategy strategy) : base(owner)
    {
      
        currentStrategy = strategy;
        originPosition = transform.position;
    }

    public override void Enter()
    {
        currentStrategy.StartMoving(transform);
    }

    public override void Execute()
    {
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
        currentStrategy.StopMoving();
    }
    public override bool CanTransition()
    {
        return true;
    }
}