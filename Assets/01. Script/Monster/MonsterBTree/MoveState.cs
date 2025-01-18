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
        animator.SetTrigger("Move"); // ������ ���� Ʈ���� ����
    }

    public override void Execute()
    {
        // �̵� ó���� ����ϰ�, ���� ��ȯ�� BehaviorTree�� �ñ�
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
        // ��� ���� ��ȯ ��� (BehaviorTree�� �Ǵ�)
        return true;
    }
}