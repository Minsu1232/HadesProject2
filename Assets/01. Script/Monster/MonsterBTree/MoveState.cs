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

        // 공격 사정거리 안에 있으면 Idle 상태로 전환
        if (distanceToPlayer <= monsterClass.CurrentAttackRange)
        {
            
            owner.ChangeState(MonsterStateType.Idle);
            return;
        }

        // 기존 상태 전환 체크와 이동 로직
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

        // 공격 사정거리 안에 있으면 이동 상태 전환을 막음 > 수정해야함
        //if (distanceToPlayer <= monsterClass.CurrentAttackRange)
        //{
        //    Debug.Log("못변해");
        //    return false;
        //}
     

        return true;
    }
}