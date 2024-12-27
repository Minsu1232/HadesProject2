using static IMonsterState;
using UnityEngine;

public class MoveState : MonsterBaseState
{
    private IMovementStrategy currentStrategy;
    private Vector3 originPosition;
    private Animator animator;
    private Vector3 lastPosition;  // 이전 프레임의 위치를 저장

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

        // 현재 이동 방향 계산
        Vector3 moveDirection = (transform.position - lastPosition).normalized;
        if (moveDirection != Vector3.zero)  // 실제로 이동 중일 때만 회전
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // 현재 위치 저장
        lastPosition = transform.position;

        // 상태 전환 체크
        if (currentStrategy.ShouldChangeState(distanceToPlayer, monsterClass))
        {
            if (distanceToPlayer <= monsterClass.CurrentAttackRange)
                owner.ChangeState(MonsterStateType.Attack);
            else if (distanceToPlayer > monsterClass.CurrentAggroDropRange)
                owner.ChangeState(MonsterStateType.Move);
            return;
        }

        // 이동 실행
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