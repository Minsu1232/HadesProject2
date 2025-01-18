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

    public override void Enter()
    {
        Debug.Log("Move State Enter");
        animator.SetTrigger("Move"); // 진입할 때만 트리거 설정
    }

    public override void Execute()
    {
        // 이동 처리만 담당하고, 상태 전환은 BehaviorTree에 맡김
        currentStrategy.Move(transform, player, monsterClass);
    }

    public override void Exit()
    {
        currentStrategy.StopMoving();
        animator.ResetTrigger("Move");
        Debug.Log("Move State Exit");
    }

    public override bool CanTransition()
    {
        // 모든 상태 전환 허용 (BehaviorTree가 판단)
        return true;
    }
}